using System.Security.Cryptography;
using FinanceBot.Data;
using FinanceBot.Models;
using FinanceBot.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FinanceBot.Services;

public class TelegramPollingWorker : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramOptions _options;
    private readonly ILogger<TelegramPollingWorker> _logger;

    public TelegramPollingWorker(
        ITelegramBotClient botClient,
        IServiceScopeFactory scopeFactory,
        IOptions<TelegramOptions> options,
        ILogger<TelegramPollingWorker> logger)
    {
        _botClient = botClient;
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        _logger.LogInformation("Starting Telegram Polling...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _botClient.ReceiveAsync(
                    updateHandler: HandleUpdateAsync,
                    errorHandler: HandleErrorAsync,
                    receiverOptions: receiverOptions,
                    cancellationToken: stoppingToken
                );
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error in Telegram polling loop. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } text } message)
            return;

        long telegramChatId = message.Chat.Id;
        string trimmedText = text.Trim();

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (trimmedText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            await HandleStartAsync(db, telegramChatId, cancellationToken);
            return;
        }

        if (trimmedText.Equals("/today", StringComparison.OrdinalIgnoreCase))
        {
            await HandleTodayAsync(db, telegramChatId, cancellationToken);
            return;
        }

        if (trimmedText.Equals("/month", StringComparison.OrdinalIgnoreCase))
        {
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            string baseUrl = config["PublicBaseUrl"] ?? "http://localhost:5080";
            await HandleMonthAsync(db, telegramChatId, baseUrl, cancellationToken);
            return;
        }

        if (SpendingParser.TryParse(trimmedText, out var parsed, out var error))
        {
            var chat = await db.Chats.FirstOrDefaultAsync(c => c.TelegramChatId == telegramChatId, cancellationToken);
            if (chat == null)
            {
                chat = new Models.Chat
                {
                    TelegramChatId = telegramChatId,
                    ReportToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
                    StartedDate = DateTime.UtcNow
                };
                db.Chats.Add(chat);
                await db.SaveChangesAsync(cancellationToken);
            }

            db.Spendings.Add(new Spending
            {
                ChatId = chat.Id,
                SpentAmount = parsed!.Amount,
                Category = parsed.Category,
                Notes = parsed.Note,
                SpentAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);
            await bot.SendMessage(telegramChatId, $"Saved {parsed.Amount:F2} {_options.Currency} {parsed.Category}", cancellationToken: cancellationToken);
        }
        else
        {
            await bot.SendMessage(telegramChatId, error ?? "Invalid format. Use: <amount> <category> [note...]", cancellationToken: cancellationToken);
        }
    }

    private async Task HandleStartAsync(AppDbContext db, long telegramChatId, CancellationToken cancellationToken)
    {
        var chat = await db.Chats.FirstOrDefaultAsync(c => c.TelegramChatId == telegramChatId, cancellationToken);
        if (chat == null)
        {
            chat = new Models.Chat
            {
                TelegramChatId = telegramChatId,
                ReportToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)),
                StartedDate = DateTime.UtcNow
            };
            db.Chats.Add(chat);
            await db.SaveChangesAsync(cancellationToken);
        }

        string msg = "Welcome to Finance Consultant Bot!\n\n" +
                     "Send spendings in format: <amount> <category> [note...]\n" +
                     "Examples:\n" +
                     "4.50 coffee\n" +
                     "32.10 groceries lidl\n\n" +
                     "Commands:\n" +
                     "/today - Today's report\n" +
                     "/month - Short monthly recap";

        await _botClient.SendMessage(telegramChatId, msg, cancellationToken: cancellationToken);
    }

    private async Task HandleTodayAsync(AppDbContext db, long telegramChatId, CancellationToken cancellationToken)
    {
        var chat = await db.Chats.FirstOrDefaultAsync(c => c.TelegramChatId == telegramChatId, cancellationToken);
        if (chat == null)
        {
            await _botClient.SendMessage(telegramChatId, "Please /start first.", cancellationToken: cancellationToken);
            return;
        }

        var todayUtc = DateTime.UtcNow.Date;
        var spendings = await db.Spendings
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= todayUtc)
            .ToListAsync(cancellationToken);

        if (!spendings.Any())
        {
            await _botClient.SendMessage(telegramChatId, "No spendings today.", cancellationToken: cancellationToken);
            return;
        }

        decimal total = spendings.Sum(s => s.SpentAmount);
        var lines = spendings.Select(s => $"- {s.SpentAmount:F2} {_options.Currency} {s.Category}" + (string.IsNullOrEmpty(s.Notes) ? "" : $" ({s.Notes})"));
        string response = $"Today total: {total:F2} {_options.Currency}\n" + string.Join("\n", lines);

        await _botClient.SendMessage(telegramChatId, response, cancellationToken: cancellationToken);
    }

    private async Task HandleMonthAsync(AppDbContext db, long telegramChatId, string baseUrl, CancellationToken cancellationToken)
    {
        var chat = await db.Chats.FirstOrDefaultAsync(c => c.TelegramChatId == telegramChatId, cancellationToken);
        if (chat == null)
        {
            await _botClient.SendMessage(telegramChatId, "Please /start first.", cancellationToken: cancellationToken);
            return;
        }

        var now = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthSpendings = await db.Spendings
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= firstDayOfMonth)
            .ToListAsync(cancellationToken);

        if (!monthSpendings.Any())
        {
            await _botClient.SendMessage(telegramChatId, "No spendings this month.", cancellationToken: cancellationToken);
            return;
        }

        string reportText = BuildDigestMessage(monthSpendings, chat.ReportToken, baseUrl, _options.Currency, now);
        await _botClient.SendMessage(telegramChatId, reportText, cancellationToken: cancellationToken);
    }

    public static string BuildDigestMessage(List<Spending> monthSpendings, string reportToken, string baseUrl, string currency, DateTime nowUtc)
    {
        var todayMidnight = nowUtc.Date;
        var last7Start = todayMidnight.AddDays(-7);
        var prev7Start = todayMidnight.AddDays(-14);

        decimal monthTotal = monthSpendings.Sum(s => s.SpentAmount);
        int monthCount = monthSpendings.Count;

        decimal last7Total = monthSpendings.Where(s => s.SpentAt >= last7Start && s.SpentAt < todayMidnight).Sum(s => s.SpentAmount);
        decimal prev7Total = monthSpendings.Where(s => s.SpentAt >= prev7Start && s.SpentAt < last7Start).Sum(s => s.SpentAmount);

        int daysElapsed = nowUtc.Day;
        decimal typicalDay = monthTotal / daysElapsed;

        var topCategories = monthSpendings
            .GroupBy(s => s.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(s => s.SpentAmount) })
            .OrderByDescending(x => x.Total)
            .Take(3)
            .ToList();

        string topCatStr = string.Join(", ", topCategories.Select(c => $"{c.Category}: {c.Total:F2} {currency}"));
        string reportUrl = $"{baseUrl.TrimEnd('/')}/report/{reportToken}";

        return $"Month total: {monthTotal:F2} {currency} ({monthCount} entries)\n" +
               $"Last 7 days: {last7Total:F2} {currency}\n" +
               $"Previous 7 days: {prev7Total:F2} {currency}\n" +
               $"Typical day: {typicalDay:F2} {currency}\n" +
               $"Top categories: {topCatStr}\n" +
               $"Detailed report: {reportUrl}";
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error handling Telegram update");
        return Task.CompletedTask;
    }
}