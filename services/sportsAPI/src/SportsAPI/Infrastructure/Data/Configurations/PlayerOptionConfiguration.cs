using Domain.Organizations.Entities;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PlayerOptionConfiguration : IEntityTypeConfiguration<PlayerOption>
{
    public void Configure(EntityTypeBuilder<PlayerOption> builder)
    {
        builder.ToTable("PlayerOptions");

        builder.HasKey(po => po.Id);

        builder.Property(po => po.Id)
            .HasConversion(
                id => id.Value,
                value => PlayerOptionId.Of(value))
            .IsRequired();

        builder.Property(po => po.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(po => po.Description)
            .HasMaxLength(1000);

        builder.Property(po => po.Votes)
            .IsRequired();

        builder.Property(po => po.ExpiresAt)
            .IsRequired();

        // Configure foreign keys
        builder.Property(po => po.PlayerId)
            .HasConversion(
                id => id.Value,
                value => PlayerId.Of(value))
            .IsRequired();

        builder.Property(po => po.OrganizationId)
            .HasConversion(
                id => id.Value,
                value => OrganizationId.Of(value))
            .IsRequired();
    }
}
