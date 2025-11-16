using Application.Common.Interfaces;
using Domain.Leagues;
using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.Player;
using Domain.PlayerOption;
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
        public DbSet<PlayerOption> PlayerOptions => Set<PlayerOption>();
        public DbSet<Theme> Themes => Set<Theme>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<League> Leagues => Set<League>();
        public DbSet<VoteAccount> VoteAccounts => Set<VoteAccount>();
        public DbSet<VoteTransaction> VoteTransactions => Set<VoteTransaction>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<OrganizationId>();
            modelBuilder.Ignore<PlayerId>();
            modelBuilder.Ignore<PlayerOptionId>();
            modelBuilder.Ignore<UserId>();
            modelBuilder.Ignore<ThemeId>();
            modelBuilder.Ignore<CodeId>();
            modelBuilder.Ignore<VoteAccountId>();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
                base.OnModelCreating(modelBuilder);
          }
        }
}
