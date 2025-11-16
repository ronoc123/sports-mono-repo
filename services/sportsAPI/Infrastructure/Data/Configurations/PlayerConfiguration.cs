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
      builder.ToTable("players");

      // Key
      builder.HasKey(p => p.Id);

      builder.Property(p => p.Id)
          .HasConversion(v => v.Value, v => PlayerId.Of(v))
          .IsRequired();

      // FKs (store IDs, no navs on ARs)
      builder.Property(p => p.LeagueId)
          .HasConversion(v => v.Value, v => LeagueId.Of(v))
          .IsRequired();

      builder.Property(p => p.OrganizationId)
          .HasConversion(
              v => v == null ? (Guid?)null : v.Value,
              v => v.HasValue ? OrganizationId.Of(v.Value) : null);
    }
  }
}
