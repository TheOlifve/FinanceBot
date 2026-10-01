using FinanceBot.Models;
using FinanceBot.Options;
using FinanceBot.Services;
using Telegram.Bot;

namespace FinanceBot.Workers;

public class TelegramDailyWorker: BackgroundService
{
    private readonly TelegramOptions _options;
    private readonly ITelegramBotClient _bot;
    private readonly ReportBuilder _reportBuilder;
    private readonly IServiceScopeFactory _scopeFactory;

    private async Task<DateTime> GetNextUpdateTime()
    {
        DateTime nowUtc = DateTime.UtcNow;
        var nextUpdate = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, _options.DigestHourUtc, 0, 0, DateTimeKind.Utc);
        
        return nextUpdate <  nowUtc ? nextUpdate.AddDays(1) : nextUpdate;
    }
    
    private string FormatDailyReport(Report report, string reportToken)
    {
        var topCategoriesText = report.CategoryTotal.Count == 0
            ? "—"
            : string.Join(", ", report.CategoryTotal.Select(c => $"{c.Key}: {c.Value:F2}"));

        return
            $"""
             📅 This month: {report.LastMonthTotal:F2} {_options.Currency} ({report.LastMonthEntries} entries)
             📆 Last 7 days: {report.LastWeekTotal:F2} {_options.Currency}
             📆 Previous 7 days: {report.PreviousWeekTotal:F2} {_options.Currency}
             📊 Typical day: {report.TypicalDayThisMonth:F2} {_options.Currency}
             🏆 Top categories: {topCategoriesText}

             🔗 Full report: .../report/{reportToken}
             """;
    }
    
    public TelegramDailyWorker(TelegramOptions options, ITelegramBotClient bot,
        ReportBuilder reportBuilder, IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _bot = bot;
        _reportBuilder = reportBuilder;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextUpdate = await GetNextUpdateTime();
            
            // await Task.Delay(nextUpdate - DateTime.UtcNow, stoppingToken);
            
            await using var scope = _scopeFactory.CreateAsyncScope();
            
            var repo = scope.ServiceProvider.GetRequiredService<ISpendingRepository>();

            ICollection<Chat> chats = await repo.GetChats(stoppingToken);

            foreach (var chat in chats)
            {

                if (chat.Spendings.Any())
                {
                    Report report = await _reportBuilder.CreateReport(chat, stoppingToken);
                    
                    await _bot.SendMessage(chat.TelegramChatId,
                        FormatDailyReport(report, ""));
                }
                
                Console.WriteLine("Daily update: " + chat.TelegramChatId);
            }
        }
    }
}