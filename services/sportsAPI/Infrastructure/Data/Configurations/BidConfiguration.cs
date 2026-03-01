using Domain.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ListingId).IsRequired();
        builder.Property(x => x.BidderId).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.Timestamp).IsRequired();

        builder.HasIndex(x => x.ListingId);
        builder.HasIndex(x => x.BidderId);
    }
}
