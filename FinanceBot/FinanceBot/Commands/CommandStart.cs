using System.Windows.Input;
using FinanceBot.DTO;
using FinanceBot.Exceptions;
using FinanceBot.Models;
using FinanceBot.Services;

namespace FinanceBot.Commands;

public class CommandStart : ITelegramCommand
{
    private readonly ISpendingRepository _spendingRepository;
    
    public CommandStart(ISpendingRepository spendingRepository)
    {
        _spendingRepository = spendingRepository;
    }
    
    public async Task<bool> Execute(RecivedMessageInfo messageInfo, CancellationToken ct)
    {
        Chat chat = await _spendingRepository.GetOrCreateChat(
            new ChatCreationDTO(messageInfo.TelegramChatId, messageInfo.SendDate),
            ct);
        
        return true;
    }
}