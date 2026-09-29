namespace FinanceBot.Exceptions;

public interface ITelegramCommand
{
    public Task<bool> Execute();
}