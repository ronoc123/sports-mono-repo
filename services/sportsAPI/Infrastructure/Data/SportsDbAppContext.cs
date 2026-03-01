using Application.Common.Interfaces;
using Domain.Abstractions;
using Domain.Leagues;
using Domain.Notification;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Player;
using Domain.PlayerOption;
using Domain.Product;
using Domain.Purchase;
using Domain.Rewards;
using Domain.Shared_kernel;
using Domain.VoteAccount;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Reflection;

namespace Infrastructure.Data
{
    public class SportsDbAppContext : DbContext, IApplicationDbContext
    {
        public SportsDbAppContext(DbContextOptions<SportsDbAppContext> options)
            : base(options) { }
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<PlayerOption> PlayerOptions => Set<PlayerOption>();
        public DbSet<Theme> Themes => Set<Theme>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<League> Leagues => Set<League>();
        public DbSet<VoteAccount> VoteAccounts => Set<VoteAccount>();
        public DbSet<VoteTransaction> VoteTransactions => Set<VoteTransaction>();
        public DbSet<RewardItem> RewardItems => Set<RewardItem>();

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Purchase> Purchases => Set<Purchase>();
        public DbSet<ProcessedWebhookEvent> ProcessedWebhookEvents => Set<ProcessedWebhookEvent>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            ApplyAuditInformation();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var events = ChangeTracker
                .Entries<IAggregate>()
                .Select(e => e.Entity.ClearDomainEvents())
                .SelectMany(e => e)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);


            if (events.Any())
            {
                var dispatcher = this.GetService<IDomainEventDispatcher>();
                await dispatcher.DispatchAsync(events, cancellationToken);
            }

            return result;
        }

        private void ApplyAuditInformation()
        {
            var utcNow = DateTime.UtcNow;

            string? currentUserId = null;

            foreach (var entry in ChangeTracker.Entries<IEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt ??= utcNow;
                    entry.Entity.CreatedBy ??= currentUserId;

                    entry.Entity.LastModified = utcNow;
                    entry.Entity.LastModifiedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Don't let EF overwrite creation info
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;

                    entry.Entity.LastModified = utcNow;
                    entry.Entity.LastModifiedBy = currentUserId;
                }
            }
        }
    }
}
