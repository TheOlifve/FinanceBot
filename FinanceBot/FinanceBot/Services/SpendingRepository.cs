using FinanceBot.Data;
using FinanceBot.DTO;
using FinanceBot.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBot.Services;

public class SpendingRepository: ISpendingRepository
{
    private readonly AppDbContext _dbContext;
    
    public SpendingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Chat> GetOrCreateChat(ChatCreationDTO chatInfo, CancellationToken ct)
    {
        Chat? chat = await _dbContext.Chats.FirstOrDefaultAsync(
            c => c.TelegramChatId == chatInfo.TelegramChatId, ct);

        if (chat != null)
            return chat;
        
        chat = new Chat(chatInfo);
        
        _dbContext.Chats.Add(chat);
        await _dbContext.SaveChangesAsync(ct);
        
        return  chat;
    }

    public async Task<Chat?> GetChat(long telegramChatId, CancellationToken ct)
    {
        return await _dbContext.Chats.FirstOrDefaultAsync(c => c.TelegramChatId == telegramChatId, ct);
    }
    
    public async Task<Chat?> GetChat(int chatId, CancellationToken ct)
    {
        return await _dbContext.Chats.FirstOrDefaultAsync(c => c.TelegramChatId == chatId, ct);
    }

    public async Task<ICollection<Chat>> GetChats(CancellationToken ct)
    {
        return await _dbContext.Chats.Include(c => c.Spendings).ToListAsync(ct);
    }

    public async Task<ICollection<Spending>> GetTodaySpendings(Chat chat, CancellationToken ct)
    {
        DateTime today = DateTime.UtcNow.Date;
        DateTime tomorrow = today.AddDays(1);
        
        return await _dbContext.Spendings.
            Where(s => s.ChatId == chat.Id && s.SpentAt >= today && s.SpentAt < tomorrow).
            ToListAsync(ct);
    }

    public async Task<ICollection<Spending>> GetMonthSpendings(Chat chat, DateTime fromUtc, CancellationToken ct)
    {
        return await _dbContext.Spendings
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= fromUtc)
            .OrderByDescending(s => s.SpentAt)
            .ToListAsync(ct);
    }

    public async Task<Spending?> CreateSpending(SpendingCreationDTO spendingInfo, CancellationToken ct)
    {
        Spending newSpending = new Spending(spendingInfo);
        
        
        Console.WriteLine($"Spending.ChatId = {newSpending.ChatId}");
        _dbContext.Spendings.Add(newSpending);
        await _dbContext.SaveChangesAsync(ct);
        
        return newSpending;
    }

    public async Task<ICollection<Spending>> GetSpendingsForMonth(int chadId, CancellationToken ct)
    {
        DateTime nowUtc = DateTime.UtcNow.Date;
        DateTime thisMonth = new DateTime(nowUtc.Year, nowUtc.Month,1, 0, 0, 0, DateTimeKind.Utc);

        return await _dbContext.Spendings.
            Where(s => s.ChatId == chadId && s.SpentAt >= thisMonth).
            ToListAsync(ct);
    }

    public async Task<ICollection<Spending>> GetSpendingsForTwoWeeks(int chadId, CancellationToken ct)
    {
        DateTime lastTwoWeeks = DateTime.UtcNow.Date.AddDays(-14);

        return await _dbContext.Spendings.
            Where(s => s.ChatId == chadId && s.SpentAt >= lastTwoWeeks).
            ToListAsync(ct);
    }
}