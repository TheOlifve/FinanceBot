using FinanceBot.DTO;

namespace FinanceBot.Models;

public class Spending
{
    public int SpendingId { get; set; }
    public int ChatId { get; set; }
    public Chat Chat { get; set; }
    public decimal SpentAmount { get; set; }
    public string Category { get; set; } = "";
    public DateTime SpentAt { get; set; }
    public string? Notes { get; set; }
    
    public Spending() {}

    public Spending(SpendingCreationDTO spendingInfo)
    {
        ChatId = spendingInfo.ChatId;
        SpentAmount = spendingInfo.SpentAmount;
        Category = spendingInfo.Category;
        SpentAt = spendingInfo.SpentAt;
        Notes = spendingInfo.Notes;
    }
}