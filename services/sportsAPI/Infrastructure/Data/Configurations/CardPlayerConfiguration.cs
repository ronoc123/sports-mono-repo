using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class CardPlayerConfiguration : IEntityTypeConfiguration<CardPlayer>
{
    public void Configure(EntityTypeBuilder<CardPlayer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LeagueId).IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Position)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.OverallRating).IsRequired();

        builder.Property(x => x.RarityTier)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.LeagueId);
    }
}
