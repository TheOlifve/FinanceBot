using System.Windows.Input;
using FinanceBot.Exceptions;
using FinanceBot.Models;

namespace FinanceBot.Commands;

public class CommandHandler
{
    private readonly IKeyedServiceProvider _provider;

    public CommandHandler(IKeyedServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task ExecuteAsync(RecivedMessageInfo messageInfo ,CancellationToken cancellationToken)
    {
        ITelegramCommand executor;
        try
        {
            executor = _provider.GetRequiredKeyedService<ITelegramCommand>(messageInfo.Message);
        }
        catch (KeyNotFoundException)
        {
            executor = _provider.GetRequiredKeyedService<ITelegramCommand>("unknown");
        }
        
        await executor.Execute(messageInfo,  cancellationToken);
    }
}