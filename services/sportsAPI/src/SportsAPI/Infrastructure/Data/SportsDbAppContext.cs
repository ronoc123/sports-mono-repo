using Application.Common.Interfaces;
using Domain.Leagues;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.SharedKernal;
using Domain.User.Entities;
using Domain.Users;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using Infrastructure.Data.VM;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Data
{
    public class SportsDbAppContext : DbContext, IApplicationDbContext
    {
        public SportsDbAppContext(DbContextOptions<SportsDbAppContext> options)
            : base(options) { }
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Code> Codes => Set<Code>();
        public DbSet<PlayerOption> PlayerOptions => Set<PlayerOption>();
        public DbSet<Theme> Themes => Set<Theme>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<League> Leagues => Set<League>();
        public DbSet<VoteAccount> VoteAccounts => Set<VoteAccount>();
        public DbSet<VoteTransaction> VoteTransactions => Set<VoteTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly ignore value object types to prevent EF Core from treating them as entities
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.OrganizationId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.LeagueId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.PlayerId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.PlayerOptionId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.CodeId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.ThemeId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.UserId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.VoteAccountId>();


      // Ignore complex value objects
      modelBuilder.Ignore<Domain.ValueObjects.TeamColors>();
                  modelBuilder.Ignore<Domain.ValueObjects.Venue>();
                  modelBuilder.Ignore<Domain.ValueObjects.MediaAssets>();
                  modelBuilder.Ignore<Domain.ValueObjects.SocialLinks>();

            // Apply entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
