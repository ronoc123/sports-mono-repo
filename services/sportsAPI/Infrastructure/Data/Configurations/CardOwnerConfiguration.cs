using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class CardOwnerConfiguration : IEntityTypeConfiguration<CardOwner>
{
    public void Configure(EntityTypeBuilder<CardOwner> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CardPlayerId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.LeagueId).IsRequired();
        builder.Property(x => x.IsListed).IsRequired();
        builder.Property(x => x.AcquiredAt).IsRequired();

        // Navigation: CardOwner → CardPlayer (no cascade delete — cards persist beyond ownership)
        builder.HasOne(x => x.CardPlayer)
            .WithMany()
            .HasForeignKey(x => x.CardPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.LeagueId });
        builder.HasIndex(x => x.CardPlayerId);
    }
}
