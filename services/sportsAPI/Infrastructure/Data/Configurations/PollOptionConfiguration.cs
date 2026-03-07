using Domain.Poll;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.ToTable("PollOptions");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasConversion(
            id => id.Value,
            value => PollOptionId.Of(value));

        builder.Property(o => o.PollId).HasConversion(
            id => id.Value,
            value => PollId.Of(value));

        builder.Property(o => o.Text).IsRequired().HasMaxLength(300);
        builder.Property(o => o.VoteCount).IsRequired();
        builder.Property(o => o.SortOrder).IsRequired();

        builder.HasIndex(o => o.PollId);
    }
}
