using Domain.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PointsEscrowConfiguration : IEntityTypeConfiguration<PointsEscrow>
{
    public void Configure(EntityTypeBuilder<PointsEscrow> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ListingId).IsRequired();
        builder.Property(x => x.HeldAmount).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasMaxLength(20);

        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasIndex(x => new { x.ListingId, x.Status });
    }
}
