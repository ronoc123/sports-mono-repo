using Domain.PlayerOption;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PlayerOptionConfiguration : IEntityTypeConfiguration<PlayerOption>
{
    public void Configure(EntityTypeBuilder<PlayerOption> builder)
    {

        builder.HasKey(x => x.Id);

        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            value => PlayerOptionId.Of(value));


        builder.Property(c => c.OrganizationId).HasConversion(
              organizationId => organizationId.Value,
              value => OrganizationId.Of(value));

        builder.Property(c => c.PlayerId).HasConversion(
            playerId => playerId.Value,
            value => PlayerId.Of(value));

  }
}
