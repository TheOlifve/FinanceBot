using FinanceBot.DTO;
using FinanceBot.Models;
using Telegram.Bot.Types;
using Chat = FinanceBot.Models.Chat;

namespace FinanceBot.Services;

public interface ISpendingRepository
{
    public Task<Chat> GetOrCreateChat(ChatCreationDTO chatInfo, CancellationToken ct);
    public Task<ICollection<Spending>> GetTodaySpendings(Chat chat, CancellationToken ct);
    public Task<ICollection<Spending>> GetMonthSpendings(Chat chat, DateTime fromUtc, CancellationToken ct);
    public Task<Chat?> GetChat(int chatId, CancellationToken ct);
    public Task<Chat?> GetChat(long telegramChatId, CancellationToken ct);
    public Task<ICollection<Chat>> GetChats(CancellationToken ct);
    public Task<Spending?> CreateSpending(SpendingCreationDTO spendingInfo, CancellationToken ct);
    public Task<ICollection<Spending>> GetSpendingsForMonth(int chadId, CancellationToken ct);
    public Task<ICollection<Spending>> GetSpendingsForTwoWeeks(int chadId, CancellationToken ct);
}