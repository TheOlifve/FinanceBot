using FinanceBot.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceBot.Data;

public class AppDbContext: DbContext
{
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Spending> Spendings { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Chat>().
            HasIndex(c => c.ReportToken).
            IsUnique();
        
        modelBuilder.Entity<Chat>().
            HasIndex(c => c.TelegramChatId).
            IsUnique();
        
        modelBuilder.Entity<Spending>().
            HasOne(s => s.Chat).
            WithMany(c => c.Spendings).
            OnDelete(DeleteBehavior.Cascade);
    }
}