using Domain.Leagues;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(c => c.Id).HasConversion(
          organizationId => organizationId.Value,
          value => LeagueId.Of(value));
  }
}
