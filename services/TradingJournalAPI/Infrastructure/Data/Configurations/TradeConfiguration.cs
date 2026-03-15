using Domain.TradeJournal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => TradeId.From(value));

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Notes).HasMaxLength(5000);

        builder.HasIndex(x => x.UserId);

        builder.HasMany(x => x.JournalEntries)
            .WithOne()
            .HasForeignKey(e => e.TradeId)
            .HasPrincipalKey(t => t.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
