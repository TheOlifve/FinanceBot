using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceBot.Data;
using FinanceBot.Models;

namespace FinanceBot.Controllers;

[AllowAnonymous]
[Route("report")]
public class ReportController : Controller
{
    private readonly AppDbContext _dbContext;

    public ReportController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("{token}")]
    public async Task<IActionResult> Get(string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return NotFound("Invalid or missing report token.");
        }

        var chat = await _dbContext.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ReportToken == token, cancellationToken);

        if (chat == null)
        {
            return NotFound("Report not found or token expired.");
        }

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1);

        var monthSpendings = await _dbContext.Spendings
            .AsNoTracking()
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= startOfMonth && s.SpentAt < endOfMonth)
            .ToListAsync(cancellationToken);

        int lastMonthEntries = monthSpendings.Count;
        decimal lastMonthTotal = monthSpendings.Sum(s => s.SpentAmount);

        var startOfLastWeek = now.Date.AddDays(-7);
        var startOfPrevWeek = now.Date.AddDays(-14);

        decimal lastWeekTotal = await _dbContext.Spendings
            .AsNoTracking()
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= startOfLastWeek)
            .SumAsync(s => s.SpentAmount, cancellationToken);

        decimal previousWeekTotal = await _dbContext.Spendings
            .AsNoTracking()
            .Where(s => s.ChatId == chat.Id && s.SpentAt >= startOfPrevWeek && s.SpentAt < startOfLastWeek)
            .SumAsync(s => s.SpentAmount, cancellationToken);

        int daysPassedInMonth = Math.Max(1, now.Day);
        decimal typicalDayThisMonth = lastMonthTotal / daysPassedInMonth;

        var categoryTotal = monthSpendings
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Category) ? "Uncategorized" : s.Category)
            .ToDictionary(g => g.Key, g => g.Sum(s => s.SpentAmount));

        string link = $"/report/{token}";

        var report = new Report(
            lastMonthEntries,
            link,
            lastWeekTotal,
            previousWeekTotal,
            lastMonthTotal,
            typicalDayThisMonth,
            categoryTotal
        );

        return View(report);
    }
}