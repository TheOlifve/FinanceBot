using FinanceBot.Models;

namespace FinanceBot.Services;

public class ReportBuilder
{
    IServiceScopeFactory _scopeFactory;

    public ReportBuilder(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    
    public async Task<Report> CreateReport(Chat chat, CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        
        ISpendingRepository repo =  scope.ServiceProvider.GetRequiredService<ISpendingRepository>();
        
        ICollection<Spending> spendingsForMonth = await repo.GetSpendingsForMonth(chat.Id, ct);
        ICollection<Spending> spendingsForTwoWeeks = await repo.GetSpendingsForTwoWeeks(chat.Id, ct);

        int thisMonthEntries =  spendingsForMonth.Count;
        decimal lastWeekTotal = 0; 
        decimal lastTwoWeeksTotal = 0;
        decimal lastMonthTotal = 0;
        
        DateTime nowUtc = DateTime.UtcNow.Date;
        DateTime lastWeekStartDate = nowUtc.AddDays(-7);

        int elapsedDaysFromThisMonth = nowUtc.Day;

        foreach (var spending in spendingsForTwoWeeks)
        {
            if (spending.SpentAt >= lastWeekStartDate)
                lastWeekTotal += spending.SpentAmount;
            lastTwoWeeksTotal += spending.SpentAmount;
        }
        
        Dictionary<string, decimal> categoryTotal = new Dictionary<string, decimal>();
        foreach (var spending in spendingsForMonth)
        {
            if (!categoryTotal.TryGetValue(spending.Category, out var current))
                current = 0;

            categoryTotal[spending.Category] = current + spending.SpentAmount;
            lastMonthTotal += spending.SpentAmount;
        }
        
        decimal typicalDayThisMonth = lastMonthTotal / elapsedDaysFromThisMonth;
        
        return new Report(thisMonthEntries, "",
            lastWeekTotal, lastTwoWeeksTotal - lastWeekTotal,
            lastMonthTotal, typicalDayThisMonth,
            categoryTotal);
    }
}