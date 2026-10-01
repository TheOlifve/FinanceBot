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
    
    public TelegramDailyWorker(TelegramOptions options, ITelegramBotClient bot, ReportBuilder reportBuilder, IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _bot = bot;
        _reportBuilder = reportBuilder;
        _scopeFactory = scopeFactory;
    }
    
    private async Task<string> CreateDailyMessage(IServiceScope scope, Chat chat, CancellationToken ct)
    {
        ISpendingRepository repo =  scope.ServiceProvider.GetRequiredService<ISpendingRepository>();
        ICollection<Spending> spendings = await repo.GetSpendingsForMonth(chat.Id, ct);

        int entries = 0;
        foreach (var spending in spendings)
        {
            entries++;
        }
        return "";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextUpdate = await GetNextUpdateTime();
            
            await Task.Delay(nextUpdate - DateTime.UtcNow, stoppingToken);
            
            await using var scope = _scopeFactory.CreateAsyncScope();
            
            var repo = scope.ServiceProvider.GetRequiredService<ISpendingRepository>();

            ICollection<Chat> chats = await repo.GetChats(stoppingToken);

            foreach (var chat in chats)
            {
                if (chat.Spendings.Any())
                    await _bot.SendMessage(chat.TelegramChatId, await CreateDailyMessage(scope, chat, stoppingToken));
                Console.WriteLine("Daily update: " + chat.TelegramChatId);
            }
        }
    }
}