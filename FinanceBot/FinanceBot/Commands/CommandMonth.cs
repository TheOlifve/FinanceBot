using System.Text;
using FinanceBot.Exceptions;
using FinanceBot.Models;
using FinanceBot.Options;
using FinanceBot.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace FinanceBot.Commands;

public class CommandMonth : ITelegramCommand
{
    private readonly ISpendingRepository _spendingRepository;
    private readonly ITelegramBotClient _bot;
    private readonly TelegramOptions _options;
    private readonly IConfiguration _configuration;

    public CommandMonth(
        ISpendingRepository spendingRepository,
        ITelegramBotClient bot,
        IOptions<TelegramOptions> options,
        IConfiguration configuration)
    {
        _spendingRepository = spendingRepository;
        _bot = bot;
        _options = options.Value;
        _configuration = configuration;
    }

    public async Task<bool> Execute(RecivedMessageInfo messageInfo, CancellationToken ct)
    {
        Chat? chat = await _spendingRepository.GetChat(messageInfo.TelegramChatId, ct);

        if (chat == null)
        {
            await _bot.SendMessage(
                messageInfo.TelegramChatId,
                "Chat not registered. Please send /start first.",
                cancellationToken: ct);
            return true;
        }

        var nowUtc = messageInfo.SendDate;
        var firstDayOfMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        ICollection<Spending> monthSpendings = await _spendingRepository.GetMonthSpendings(
            chat,
            firstDayOfMonth,
            ct);

        if (monthSpendings.Count == 0)
        {
            await _bot.SendMessage(
                messageInfo.TelegramChatId,
                "No spendings recorded for this month.",
                cancellationToken: ct);
            return true;
        }

        string baseUrl = _configuration["PublicBaseUrl"] ?? "http://localhost:5080";
        string responseText = BuildDigestMessage(monthSpendings, chat.ReportToken, baseUrl, _options.Currency, nowUtc);

        await _bot.SendMessage(messageInfo.TelegramChatId, responseText, cancellationToken: ct);
        return true;
    }

    private static string BuildDigestMessage(
        ICollection<Spending> monthSpendings,
        string reportToken,
        string baseUrl,
        string currency,
        DateTime nowUtc)
    {
        var todayMidnight = nowUtc.Date;
        var last7Start = todayMidnight.AddDays(-7);
        var prev7Start = todayMidnight.AddDays(-14);

        decimal monthTotal = monthSpendings.Sum(s => s.SpentAmount);
        int monthCount = monthSpendings.Count;

        decimal last7Total = monthSpendings
            .Where(s => s.SpentAt >= last7Start && s.SpentAt < todayMidnight)
            .Sum(s => s.SpentAmount);

        decimal prev7Total = monthSpendings
            .Where(s => s.SpentAt >= prev7Start && s.SpentAt < last7Start)
            .Sum(s => s.SpentAmount);

        int daysElapsed = nowUtc.Day;
        decimal typicalDay = daysElapsed > 0 ? monthTotal / daysElapsed : monthTotal;

        var topCategories = monthSpendings
            .GroupBy(s => s.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(s => s.SpentAmount) })
            .OrderByDescending(x => x.Total)
            .Take(3);

        string topCatStr = string.Join(", ", topCategories.Select(c => $"{c.Category}: {c.Total:F2} {currency}"));
        string reportUrl = $"{baseUrl.TrimEnd('/')}/report/{reportToken}";

        var sb = new StringBuilder();
        sb.AppendLine($"Month total: {monthTotal:F2} {currency} ({monthCount} entries)");
        sb.AppendLine($"Last 7 days: {last7Total:F2} {currency}");
        sb.AppendLine($"Previous 7 days: {prev7Total:F2} {currency}");
        sb.AppendLine($"Typical day: {typicalDay:F2} {currency}");
        sb.AppendLine($"Top categories: {topCatStr}");
        sb.AppendLine($"Detailed report: {reportUrl}");

        return sb.ToString();
    }
}