namespace FinanceBot.Models;

public class Report
{
    public int LastMonthEntries { get; set; }
    public string Link {get; set;}
    public decimal LastWeekTotal {get; set;}
    public decimal PreviousWeekTotal {get; set;}
    public decimal LastMonthTotal {get; set;}
    public decimal TypicalDayThisMonth {get; set;}
    public Dictionary<string, decimal> CategoryTotal {get; set;}

    public Report(int lastMonthEntries, string link, 
        decimal lastWeekTotal, decimal previousWeekTotal,
        decimal lastMonthTotal, decimal typicalDayThisMonth,
        Dictionary<string, decimal> categoryTotal)
    {
        LastMonthEntries = lastMonthEntries;
        Link = link;
        LastWeekTotal = lastWeekTotal;
        PreviousWeekTotal = previousWeekTotal;
        LastMonthTotal = lastMonthTotal;
        TypicalDayThisMonth = typicalDayThisMonth;
        CategoryTotal = categoryTotal;
    }
}