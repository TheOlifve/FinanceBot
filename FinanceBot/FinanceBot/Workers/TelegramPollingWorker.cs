using FinanceBot.Commands;
using FinanceBot.DTO;
using FinanceBot.Exceptions;
using FinanceBot.Models;
using FinanceBot.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class TelegramPollingWorker : BackgroundService
{
    private readonly ITelegramBotClient _bot;
    private readonly IServiceScopeFactory _scopeFactory;

    public TelegramPollingWorker(ITelegramBotClient bot, IServiceScopeFactory scopeFactory)
    {
        _bot = bot;
        _scopeFactory = scopeFactory;
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
        await using var scope = _scopeFactory.CreateAsyncScope();
        
        try
        {
            if (update.Message?.Text is null)
                return;
            
            if (update.Message.Text.StartsWith("/"))
            {
                var handler = scope.
                    ServiceProvider.
                    GetRequiredKeyedService<ITelegramCommand>(update.Message.Text);
                
                await handler.Execute(new RecivedMessageInfo(update), cancellationToken);
            }
            else
            {
                var parser = scope.ServiceProvider.GetRequiredService<ISpendingParser>();

                var parsedSpending = await parser.Parse(update.Message.Text);

                var repo = scope.ServiceProvider.GetRequiredService<ISpendingRepository>();

                var chat = await repo.GetChat(update.Message.Chat.Id, cancellationToken);

                var spending = await repo.CreateSpending(
                    new SpendingCreationDTO(chat.Id, update.Message.Date, parsedSpending),
                    cancellationToken);

                if (spending != null)
                {
                    string noteText = string.IsNullOrEmpty(spending.Notes) ? "" : $" ({spending.Notes})";
                    string replyText = $"Saved: {spending.SpentAmount:F2} {spending.Category}{noteText}";

                    await bot.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: replyText,
                        cancellationToken: cancellationToken);
                }

            }
        }
        catch (ParserException exception)
        {
            Console.WriteLine(exception.Message);
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