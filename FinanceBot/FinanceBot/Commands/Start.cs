using System.Windows.Input;
using FinanceBot.Exceptions;
using FinanceBot.Services;

namespace FinanceBot.Commands;

public class Start: ITelegramCommand
{
    private readonly ISpendingRepository _spendingRepository;
    
    public Start(ISpendingRepository spendingRepository)
    {
        _spendingRepository = spendingRepository;
    }
    
    public async Task<bool> Execute()
    {
        return true;
    }
}