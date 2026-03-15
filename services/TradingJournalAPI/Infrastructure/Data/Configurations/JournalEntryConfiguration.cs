using Domain.TradeJournal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TradeId)
            .HasConversion(id => id.Value, value => TradeId.From(value));

        builder.Property(x => x.Phase).IsRequired().HasConversion<string>();
        builder.Property(x => x.EmotionalState).IsRequired().HasConversion<string>();
        builder.Property(x => x.IsEffortless).IsRequired();
        builder.Property(x => x.FreeFormNotes).HasMaxLength(5000);
        builder.Property(x => x.AISessionId);

        builder.HasIndex(x => new { x.TradeId, x.Phase }).IsUnique();
    }
}
