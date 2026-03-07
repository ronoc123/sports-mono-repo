using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class TriviaAnswerConfiguration : IEntityTypeConfiguration<TriviaAnswer>
{
    public void Configure(EntityTypeBuilder<TriviaAnswer> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasConversion(
            id => id.Value,
            value => TriviaAnswerId.Of(value));

        builder.Property(a => a.TriviaQuestionId).HasConversion(
            id => id.Value,
            value => TriviaQuestionId.Of(value));

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.SelectedOption).IsRequired().HasMaxLength(500);
        builder.Property(a => a.IsCorrect).IsRequired();
        builder.Property(a => a.AnsweredAt).IsRequired();

        // Idempotency: one answer per user per question
        builder.HasIndex(a => new { a.UserId, a.TriviaQuestionId }).IsUnique();

        builder.HasIndex(a => a.TriviaQuestionId);
    }
}
