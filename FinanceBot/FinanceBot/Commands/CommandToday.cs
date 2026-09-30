using System.Text;
using FinanceBot.Exceptions;
using FinanceBot.Models;
using FinanceBot.Services;
using Telegram.Bot;

namespace FinanceBot.Commands;

public class CommandToday: ITelegramCommand
{
    private readonly ISpendingRepository _spendingRepository;
    private readonly ITelegramBotClient  _bot;
    
    public CommandToday(ISpendingRepository spendingRepository, ITelegramBotClient bot)
    {
        _spendingRepository = spendingRepository;
        _bot = bot;
    }
    public async Task<bool> Execute(RecivedMessageInfo chatInfo, CancellationToken ct)
    {
        Chat? chat = await _spendingRepository.GetChat(chatInfo.TelegramChatId, ct);
        
        if (chat == null)
            return true;
        
        ICollection<Spending> spendings = await _spendingRepository.GetTodaySpendings(chat, ct);

        decimal sum = 0;
        StringBuilder response = new StringBuilder();

        foreach (var s in spendings)
        {
            response.AppendLine($"{s.Category} - {s.SpentAmount}");
            sum += s.SpentAmount;   
        }
        response.AppendLine($"Today sum: {sum}");
        
        await _bot.SendMessage(chatInfo.TelegramChatId, response.ToString());
        return true;
    }
}