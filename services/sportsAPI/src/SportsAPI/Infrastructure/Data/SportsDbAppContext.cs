using Microsoft.EntityFrameworkCore;
using Domain.Leagues;
using Domain.User.Entities;
using Domain.Organizations.Entities;
using Domain.SharedKernal;
using Domain.Users;
using Domain.Organizations;
using System.Reflection;
using Application.Common.Interfaces;

namespace Infrastructure.Data
{
    public class SportsDbAppContext : DbContext, IApplicationDbContext
    {
        public SportsDbAppContext(DbContextOptions<SportsDbAppContext> options)
            : base(options) { }
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Code> Codes => Set<Code>();
        public DbSet<PlayerOption> PlayerOptions => Set<PlayerOption>();
        public DbSet<Vote> Votes => Set<Vote>();
        public DbSet<Theme> Themes => Set<Theme>();
        public DbSet<Player> Players => Set<Player>();
        public DbSet<UserVotes> UserVotes => Set<UserVotes>();
        public DbSet<League> Leagues => Set<League>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Explicitly ignore value object types to prevent EF Core from treating them as entities
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.OrganizationId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.LeagueId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.PlayerId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.PlayerOptionId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.UserId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.CodeId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.VoteId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.UserVotesId>();
            modelBuilder.Ignore<Domain.ValueObjects.ConcreteTypes.ThemeId>();

            // Ignore value objects
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
