using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserCardConfiguration : IEntityTypeConfiguration<UserCard>
{
    public void Configure(EntityTypeBuilder<UserCard> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId).IsRequired();
        builder.Property(u => u.CardPlayerId).IsRequired();
        builder.Property(u => u.CardPackId).IsRequired();
        builder.Property(u => u.OrgId).IsRequired();
        builder.Property(u => u.LeagueId).IsRequired();
        builder.Property(u => u.RarityTier).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PullSeed).HasMaxLength(64).IsRequired();
        builder.Property(u => u.IsListed).IsRequired();

        // FK to CardPack
        builder.HasOne<CardPack>()
            .WithMany()
            .HasForeignKey(u => u.CardPackId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to CardPlayer (Restrict — cards survive player entry deletions)
        builder.HasOne(u => u.CardPlayer)
            .WithMany()
            .HasForeignKey(u => u.CardPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => new { u.UserId, u.OrgId });
        builder.HasIndex(u => u.CardPackId);
    }
}
