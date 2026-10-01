using FinanceBot.Commands;
using FinanceBot.DTO;
using FinanceBot.Exceptions;
using FinanceBot.Models;
using FinanceBot.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FinanceBot.Controllers;

[ApiController]
[Route("api/bot")]
public class TelegramBotController : ControllerBase
{
    private readonly ITelegramBotClient _bot;

    public TelegramBotController(ITelegramBotClient bot)
    {
        _bot = bot;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Update update, [FromServices] IServiceScopeFactory scopeFactory, CancellationToken cancellationToken)
    {
        if (update.Message?.Text is null)
            return Ok();

        await using var scope = scopeFactory.CreateAsyncScope();

        try
        {
            if (update.Message.Text.StartsWith("/"))
            {
                var handler = scope.ServiceProvider.
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
                    await _bot.SendMessage(
                        chatId: update.Message.Chat.Id,
                        text: $"Saved: {spending.SpentAmount:F2} {spending.Category}{noteText}",
                        cancellationToken: cancellationToken);
                }
            }
        }
        catch (ParserException exception)
        {
            Console.WriteLine(exception.Message);
        }

        return Ok();
    }
}