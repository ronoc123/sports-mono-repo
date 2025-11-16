using Domain.PlayerOption;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PlayerOptionConfiguration : IEntityTypeConfiguration<PlayerOption>
{
    public void Configure(EntityTypeBuilder<PlayerOption> builder)
    {
        builder.ToTable("player_options");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
          .HasColumnName("id")
          .HasConversion(v => v.Value, v => PlayerOptionId.Of(v))
          .IsRequired();

        builder.Property(x => x.OrganizationId)
          .HasColumnName("organization_id")
          .HasConversion(v => v.Value, v => OrganizationId.Of(v))
          .IsRequired();

        builder.Property(x => x.PlayerId)
          .HasColumnName("player_id")
          .HasConversion(v => v.Value, v => PlayerId.Of(v))
          .IsRequired();

        builder.Ignore(x => x.VoteHistory);
  }
}
