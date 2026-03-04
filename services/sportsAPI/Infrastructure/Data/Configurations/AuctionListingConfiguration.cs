using Domain.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class AuctionListingConfiguration : IEntityTypeConfiguration<AuctionListing>
{
    public void Configure(EntityTypeBuilder<AuctionListing> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserCardId).IsRequired();
        builder.Property(x => x.SellerId).IsRequired();
        builder.Property(x => x.LeagueId).IsRequired();
        builder.Property(x => x.StartingBid).IsRequired();
        builder.Property(x => x.BuyNowPrice);
        builder.Property(x => x.CurrentBid).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ExpiresAt).IsRequired();

        builder.HasIndex(x => new { x.LeagueId, x.Status });
        builder.HasIndex(x => x.SellerId);
        builder.HasIndex(x => x.ExpiresAt);
    }
}
