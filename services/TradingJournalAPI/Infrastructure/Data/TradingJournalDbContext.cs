using Domain.Abstractions;
using Domain.AIConversation;
using Domain.BacktestIntegrity;
using Domain.BiweeklyReview;
using Domain.TradeJournal;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Data;

public class TradingJournalDbContext : DbContext
{
    public TradingJournalDbContext(DbContextOptions<TradingJournalDbContext> options)
        : base(options) { }

    // BacktestIntegrity
    public DbSet<BacktestSession> BacktestSessions => Set<BacktestSession>();
    public DbSet<IntegrityAnswer> IntegrityAnswers => Set<IntegrityAnswer>();

    // TradeJournal
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    // AIConversation
    public DbSet<AISession> AISessions => Set<AISession>();
    public DbSet<AIMessage> AIMessages => Set<AIMessage>();

    // BiweeklyReview
    public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();
    public DbSet<ReviewMessage> ReviewMessages => Set<ReviewMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt ??= utcNow;
                entry.Entity.LastModified = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.CreatedAt).IsModified = false;
                entry.Entity.LastModified = utcNow;
            }
        }
    }
}
