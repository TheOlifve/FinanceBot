using FinanceBot.DTO;
using FinanceBot.Models;
using Telegram.Bot.Types;
using Chat = FinanceBot.Models.Chat;

namespace FinanceBot.Services;

public interface ISpendingRepository
{
    public Task<Chat> GetOrCreateChat(ChatCreationDTO chatInfo, CancellationToken ct);
    public Task<ICollection<Spending>> GetTodaySpendings(Chat chat, CancellationToken ct);
    public Task<Chat?> GetChat(long chatId, CancellationToken ct);
    // public Task<Spending?> CreateSpending(long chatId, CancellationToken ct);
}