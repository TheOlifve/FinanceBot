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
}