using Domain.SharedKernal;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CodeConfiguration : IEntityTypeConfiguration<Code>
{
    public void Configure(EntityTypeBuilder<Code> builder)
    {
        builder.ToTable("Codes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CodeId.Of(value))
            .IsRequired();

        builder.Property(c => c.VotesAwarded)
            .IsRequired();

        builder.Property(c => c.IsRedeemed)
            .IsRequired();

        builder.Property(c => c.RedeemedAt);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        // Configure foreign keys
        builder.Property(c => c.OrganizationId)
            .HasConversion(
                id => id.Value,
                value => OrganizationId.Of(value))
            .IsRequired();

        builder.Property(c => c.RedeemerId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? UserId.Of(value.Value) : null);

        // Configure relationships - temporarily commented out to avoid EF Core confusion
        // TODO: Configure relationships properly when navigation properties are set up
        // builder.HasOne<Domain.Organizations.Organization>()
        //     .WithMany()
        //     .HasForeignKey(c => c.OrganizationId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // Add indexes for performance
        builder.HasIndex(c => c.OrganizationId);
        builder.HasIndex(c => c.IsRedeemed);
        builder.HasIndex(c => c.CreatedAt);
    }
}
