using System.Text.RegularExpressions;

namespace FinanceBot.Services;

public class ParsedSpending
{
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public static class SpendingParser
{
    private static readonly Regex CategoryRegex = new(@"^[a-zA-Z]+$", RegexOptions.Compiled);

    public static bool TryParse(string input, out ParsedSpending? result, out string? errorMessage)
    {
        result = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            errorMessage = "Format: <amount> <category> [note...]";
            return false;
        }

        var parts = input.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            errorMessage = "Format: <amount> <category> [note...]";
            return false;
        }

        string rawAmount = parts[0].Replace(',', '.');
        if (!decimal.TryParse(rawAmount, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) || amount <= 0)
        {
            errorMessage = "Amount must be a positive decimal number without currency symbols.";
            return false;
        }

        string category = parts[1].ToLowerInvariant();
        if (!CategoryRegex.IsMatch(category))
        {
            errorMessage = "Category must be one word containing only letters.";
            return false;
        }

        string? note = parts.Length > 2 ? parts[2].Trim() : null;

        result = new ParsedSpending
        {
            Amount = amount,
            Category = category,
            Note = note
        };

        return true;
    }
}