using FinanceBot.Models;
using FinanceBot.Options;
using FinanceBot.Services;
using Telegram.Bot;

namespace FinanceBot.Workers;

public class TelegramDailyWorker: BackgroundService
{
    private readonly TelegramOptions _options;
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    private async Task<DateTime> GetNextUpdateTime()
    {
        DateTime nowUtc = DateTime.UtcNow;
        var nextUpdate = new DateTime(nowUtc.Year, nowUtc.Month, nowUtc.Day, _options.DigestHourUtc, 0, 0, DateTimeKind.Utc);
        
        return nextUpdate <  nowUtc ? nextUpdate.AddDays(1) : nextUpdate;
    } 
    
    public TelegramDailyWorker(TelegramOptions options, ITelegramBotClient bot, IServiceScopeFactory scopeFactory)
    {
        _options = options;
        _bot = bot;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextUpdate = await GetNextUpdateTime();
            Console.WriteLine($"Daily update: {nextUpdate}");
            
            await Task.Delay(nextUpdate - DateTime.UtcNow, stoppingToken);
            
            await using var scope = _scopeFactory.CreateAsyncScope();
            
            var repo = scope.ServiceProvider.GetRequiredService<ISpendingRepository>();

            ICollection<Chat> chats = await repo.GetChats(stoppingToken);

            foreach (var chat in chats)
            {
                await _bot.SendMessage(chat.TelegramChatId, "test daily"); 
                Console.WriteLine("Daily update: " + chat.TelegramChatId);
            }
        }
    }
}