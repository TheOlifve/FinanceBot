using FinanceBot.Services;

namespace FinanceBot.DTO;

public class SpendingCreationDTO
{
    public int ChatId { get; set; }
    public decimal SpentAmount { get; set; }
    public string Category { get; set; } = "";
    public DateTime SpentAt { get; set; }
    public string? Notes { get; set; }

    public SpendingCreationDTO(int chatId, DateTime spentAt, ParsedSpending parsedSpending)
    {
        ChatId = chatId;
        SpentAt = spentAt;
        SpentAmount = parsedSpending.Amount;
        Category = parsedSpending.Title;
        Notes = parsedSpending.Description;
    }
}