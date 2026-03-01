using Domain.H2H;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class H2HSquadCardConfiguration : IEntityTypeConfiguration<H2HSquadCard>
{
    public void Configure(EntityTypeBuilder<H2HSquadCard> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.MatchId).IsRequired();
        builder.Property(s => s.CardOwnerId).IsRequired();
        builder.Property(s => s.SlotIndex).IsRequired();

        builder.HasIndex(s => s.MatchId);

        builder.ToTable("H2HSquadCards");
    }
}
