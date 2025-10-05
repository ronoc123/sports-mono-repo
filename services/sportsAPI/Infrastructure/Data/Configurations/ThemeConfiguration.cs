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
        builder.ToTable("Themes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => ThemeId.Of(value))
            .IsRequired();

        builder.Property(t => t.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.ColorPrimary)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.ColorSecondary)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.ColorTertiary)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Logo)
            .HasMaxLength(500);

        // Add unique constraint on name
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
