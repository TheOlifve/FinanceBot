using FinanceBot.Exceptions;
using FinanceBot.Options;

namespace FinanceBot.Services;

public class ParsedSpending
{
    public decimal Amount { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

public class SpendingParser: ISpendingParser
{
    private const string FormatHint = "Format: <amount> <category> [note], e.g. 4.50 coffee";

    public async Task<ParsedSpending> Parse(string input)
    {
        string[] tokens = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (tokens.Length < 2)
            throw new InvalidArgumentsException($"Invalid number of parameters {input.Length}. Should be at least 2. {FormatHint}");

        ParsedSpending result = new ParsedSpending();
        
        if (!decimal.TryParse(tokens[0].Replace('.', ','), out decimal resultAmount))
            throw new InvalidAmountException($"Amount must be a number and come first. {FormatHint}");
        if (resultAmount <= 0)
            throw new InvalidAmountException($"Amount must be greater than zero. {FormatHint}");
        
        string description = string.Join(" ", tokens.Skip(2));

        result.Amount = resultAmount;
        result.Title = tokens[1].ToLowerInvariant();
        result.Description = description.Length > 0 ? description : null; 
        
        return result;
    }
}