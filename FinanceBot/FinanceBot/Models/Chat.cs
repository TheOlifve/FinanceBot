using FinanceBot.DTO;

namespace FinanceBot.Models;

public class Chat
{
    public int Id { get; set; }
    public long TelegramChatId { get; set; }
    public string ReportToken { get; set; } = "";
    public DateTime StartedDate { get; set; }
    public ICollection<Spending> Spendings { get; set; } = new List<Spending>();

    public Chat() { }
    public Chat(ChatCreationDTO chatDTO)
    {
        TelegramChatId = chatDTO.TelegramChatId;
        ReportToken = chatDTO.ReportToken;
        StartedDate = chatDTO.StartedDate;
    }
}