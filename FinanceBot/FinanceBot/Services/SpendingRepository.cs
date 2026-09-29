using FinanceBot.Data;

namespace FinanceBot.Services;

public class SpendingRepository: ISpendingRepository
{
    private readonly AppDbContext _dbContext; 
    
    public SpendingRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
}