using System.Windows.Input;
using FinanceBot.DTO;
using FinanceBot.Exceptions;
using FinanceBot.Models;
using FinanceBot.Services;
using Telegram.Bot;

namespace FinanceBot.Commands;

public class CommandStart : ITelegramCommand
{
    private readonly ISpendingRepository _spendingRepository;
    private readonly ITelegramBotClient  _bot;
    
    private const string Usage = """
                                 *Welcome!* This chat is saved, and I'll track your spending here.

                                 *How to log an expense*
                                 Send a message: `amount description`

                                 *Valid*
                                 • `4.50 coffee`
                                 • `32.10 groceries lidl`
                                 • `120 rent`
                                 • `0.80 bus`

                                 *Invalid*
                                 • `coffee 4.50` — amount must come first
                                 • `-3 food` — amount must be positive

                                 If a message is invalid, I'll reply with a brief explanation.

                                 *Commands*
                                 /start - Save this chat and show this message
                                 /today - Total and list of today's expenses
                                 /month - Short recap of the current month
                                 """;
    
    public CommandStart(ISpendingRepository spendingRepository, ITelegramBotClient bot)
    {
        _spendingRepository = spendingRepository;
        _bot = bot;
    }
    
    public async Task<bool> Execute(RecivedMessageInfo messageInfo, CancellationToken ct)
    {
        Chat chat = await _spendingRepository.GetOrCreateChat(
            new ChatCreationDTO(messageInfo.TelegramChatId, messageInfo.SendDate),
            ct);
        
        await _bot.SendMessage(messageInfo.TelegramChatId, Usage);        
        return true;
    }
}