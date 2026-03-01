using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RarityTierConfigConfiguration : IEntityTypeConfiguration<RarityTierConfig>
{
    public void Configure(EntityTypeBuilder<RarityTierConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RarityName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OrgId)
            .IsRequired();

        builder.Property(x => x.RatingMin).IsRequired();
        builder.Property(x => x.RatingMax).IsRequired();
        builder.Property(x => x.PullWeightBps).IsRequired();

        builder.HasIndex(x => new { x.OrgId, x.RarityName }).IsUnique();
    }
}
