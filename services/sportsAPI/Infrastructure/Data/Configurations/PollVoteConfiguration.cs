using Domain.Poll;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PollVoteConfiguration : IEntityTypeConfiguration<PollVote>
{
    public void Configure(EntityTypeBuilder<PollVote> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasConversion(
            id => id.Value,
            value => PollVoteId.Of(value));

        builder.Property(v => v.PollId).HasConversion(
            id => id.Value,
            value => PollId.Of(value));

        builder.Property(v => v.PollOptionId).HasConversion(
            id => id.Value,
            value => PollOptionId.Of(value));

        builder.Property(v => v.UserId).IsRequired();
        builder.Property(v => v.VotedAt).IsRequired();

        // Idempotency: one vote per user per poll
        builder.HasIndex(v => new { v.UserId, v.PollId }).IsUnique();

        builder.HasIndex(v => v.PollId);
    }
}
