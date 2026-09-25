namespace FinanceBot.Models;

public class Chat
{
    public int Id { get; set; }
    public long TelegramChatId { get; set; }
    public string ReportToken { get; set; } = "";
    public DateTime StartedDate { get; set; }
    public ICollection<Spending> Spendings { get; set; } = new List<Spending>();
}