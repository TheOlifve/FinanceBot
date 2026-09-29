namespace FinanceBot.DTO;

public class ChatCreationDTO
{
    public long TelegramChatId { get; set; }
    public string ReportToken { get; set; } = "";
    public DateTime StartedDate { get; set; }

    public ChatCreationDTO(long telegramChatId, DateTime startedDate)
    {
        TelegramChatId = telegramChatId;
        StartedDate = startedDate;
        ReportToken = "";
    }
}