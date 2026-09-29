using FinanceBot.Models;

namespace FinanceBot.Exceptions;

public interface ITelegramCommand
{
    public Task<bool> Execute(RecivedMessageInfo chatId, CancellationToken ct);
}