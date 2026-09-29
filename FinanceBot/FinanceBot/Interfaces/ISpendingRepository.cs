using FinanceBot.DTO;
using FinanceBot.Models;

namespace FinanceBot.Services;

public interface ISpendingRepository
{
    public Task<Chat> GetOrCreateChat(ChatCreationDTO chatInfo, CancellationToken ct);
}