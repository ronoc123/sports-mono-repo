using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class TriviaQuestionConfiguration : IEntityTypeConfiguration<TriviaQuestion>
{
    public void Configure(EntityTypeBuilder<TriviaQuestion> builder)
    {
        builder.ToTable("TriviaQuestions");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id).HasConversion(
            id => id.Value,
            value => TriviaQuestionId.Of(value));

        builder.Property(q => q.SeriesId).HasConversion(
            id => id.Value,
            value => TriviaSeriesId.Of(value));

        builder.Property(q => q.QuestionText).IsRequired().HasMaxLength(1000);
        builder.Property(q => q.OptionsJson).IsRequired();
        builder.Property(q => q.CorrectOption).IsRequired().HasMaxLength(500);
        builder.Property(q => q.VoteReward).IsRequired();
        builder.Property(q => q.Status).IsRequired().HasConversion<int>();
        builder.Property(q => q.AnswerCount).IsRequired();
        builder.Property(q => q.CreatedAt).IsRequired();

        builder.HasIndex(q => new { q.SeriesId, q.Status });
    }
}
