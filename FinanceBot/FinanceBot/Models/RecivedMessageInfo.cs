using Telegram.Bot.Types;

namespace FinanceBot.Models;

public class RecivedMessageInfo
{
    public string Message { get; set; }
    public long TelegramChatId { get; set; }
    public DateTime SendDate { get; set; }

    public RecivedMessageInfo(Update update)
    {
        Message = update.Message!.Text!;
        TelegramChatId = update.Message.Chat?.Id ?? 0;
        SendDate = update.Message.Date;
    }
}