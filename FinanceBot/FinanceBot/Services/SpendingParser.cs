using FinanceBot.Options;

namespace FinanceBot.Services;

public class ParsedSpending
{
    public decimal Amount { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}

public static class SpendingParser
{
    public const string FormatHint = "Format: <amount> <category> [note], e.g. 4.50 coffee";

    public static ParsedSpending Parse(string input)
    {
        string[] tokens = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (tokens.Length < 2)
            throw new ArgumentException($"Invalid number of parameters {input.Length}. Should be at least 2. {FormatHint}");

        ParsedSpending result = new ParsedSpending();

        if (!decimal.TryParse(tokens[0].Replace(',', '.'), out decimal amount))
            throw new ArgumentException($"Amount must be a number and come first. {FormatHint}");
        if (amount <= 0)
            throw new ArgumentException($"Amount must be greater than zero. {FormatHint}");
        
        string description = string.Join(" ", tokens.Skip(2));

        result.Amount = amount;
        result.Title = tokens[1].ToLowerInvariant();
        result.Description = description.Length > 0 ? description : null; 
        
        return result;
    }
}