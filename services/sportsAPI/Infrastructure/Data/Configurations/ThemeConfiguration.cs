using Domain.Organizations;
using Domain.Organizations.Entities;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {

        builder.HasKey(t => t.Id);

        builder.Property(c => c.Id).HasConversion(
          id => id.Value,
          value => ThemeId.Of(value));
  }
}
