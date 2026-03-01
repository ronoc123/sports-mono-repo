using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class CardPackConfiguration : IEntityTypeConfiguration<CardPack>
{
    public void Configure(EntityTypeBuilder<CardPack> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.OrgId).IsRequired();
        builder.Property(p => p.LeagueId).IsRequired();
        builder.Property(p => p.PointCost).IsRequired();
        builder.Property(p => p.PullSeed).HasMaxLength(64).IsRequired();

        builder.HasIndex(p => new { p.UserId, p.OrgId });
        builder.HasIndex(p => p.LeagueId);
    }
}
