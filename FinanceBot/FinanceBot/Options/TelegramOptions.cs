namespace FinanceBot.Options;

public class TelegramOptions
{
    public const string SectionName = "Telegram";
    public string BotToken { get; set; } = "";
    public string Currency { get; set; } = "";
    public int DigestHourUtc { get; set; } = 18;
}