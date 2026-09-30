namespace FinanceBot.Services;

public interface ISpendingParser
{
    public Task<ParsedSpending> Parse(string input);
}