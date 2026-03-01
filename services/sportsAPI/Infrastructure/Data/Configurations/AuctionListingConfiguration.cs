using Domain.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class AuctionListingConfiguration : IEntityTypeConfiguration<AuctionListing>
{
    public void Configure(EntityTypeBuilder<AuctionListing> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardOwnerId).IsRequired();
        builder.Property(x => x.SellerId).IsRequired();
        builder.Property(x => x.OrgId).IsRequired();
        builder.Property(x => x.StartingBid).IsRequired();
        builder.Property(x => x.BuyNowPrice);
        builder.Property(x => x.CurrentBid).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasIndex(x => new { x.OrgId, x.Status });
        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => x.ExpiresAt);
    }
}
