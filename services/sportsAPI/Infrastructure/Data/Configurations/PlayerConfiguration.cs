using Domain.Player;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
  public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
  {
    public void Configure(EntityTypeBuilder<Player> builder)
    {

      // Key
      builder.HasKey(p => p.Id);

      builder.Property(c => c.Id).HasConversion(
        id => id.Value,
        value => PlayerId.Of(value));

      builder.Property(c => c.LeagueId).HasConversion(
        leagueId => leagueId.Value,
        value => LeagueId.Of(value));

      builder.Property(p => p.OrganizationId)
          .HasConversion(
              v => v == null ? (Guid?)null : v.Value,
              v => v.HasValue ? OrganizationId.Of(v.Value) : null);
    }
  }
}
