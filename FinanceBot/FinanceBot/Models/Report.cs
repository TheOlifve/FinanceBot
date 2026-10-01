namespace FinanceBot.Models;

public class Report
{
    public int ThisMonthEntries { get; set; }
    public string Link {get; set;}
    public decimal LastWeekTotal {get; set;}
    public decimal PreviousWeekTotal {get; set;}
    public decimal LastMonthTotal {get; set;}
    public decimal TypicalDayThisMonth {get; set;}
    public Dictionary<string, decimal> CategoryTotal {get; set;}

    public Report(int thisMonthEntries, string link, 
        decimal lastWeekTotal, decimal previousWeekTotal,
        decimal lastMonthTotal, decimal typicalDayThisMonth,
        Dictionary<string, decimal> categoryTotal)
    {
        ThisMonthEntries = thisMonthEntries;
        Link = link;
        LastWeekTotal = lastWeekTotal;
        PreviousWeekTotal = previousWeekTotal;
        LastMonthTotal = lastMonthTotal;
        TypicalDayThisMonth = typicalDayThisMonth;
        CategoryTotal = categoryTotal;
    }
}