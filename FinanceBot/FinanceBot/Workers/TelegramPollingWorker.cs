using FinanceBot.Exceptions;
using FinanceBot.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramPollingWorker : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly ISpendingParser    _spendingParser;

    public TelegramPollingWorker(ITelegramBotClient bot, ISpendingParser spendingParser)
    {
        _bot = bot;
        _spendingParser = spendingParser;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message]
        };

        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        var me = await _bot.GetMe(stoppingToken);

        Console.WriteLine($"Bot @{me.Username} started");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient bot,
        Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message?.Text is not null)
            {
                await _spendingParser.Parse(update.Message.Text);
            }
        }
        catch (ParserException exception)
        {
            Console.WriteLine(exception.Message);
            await _bot.SendMessage(update.Message!.Chat.Id, exception.Message);
        }

        await Task.CompletedTask;
    }

    private Task HandleErrorAsync(
        ITelegramBotClient bot,
        Exception exception,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Telegram error: {exception.Message}");

        return Task.CompletedTask;
    }
}