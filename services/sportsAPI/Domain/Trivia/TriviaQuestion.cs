using BuildingBlocks.Exceptions;

namespace Domain.Trivia;

/// <summary>
/// A single trivia question belonging to a TriviaSeries.
/// Status lifecycle: Pending → Active → Archived (no reverse transitions).
/// </summary>
public sealed class TriviaQuestion
{
    internal TriviaQuestion() { } // EF

    public TriviaQuestionId Id { get; private set; } = null!;
    public TriviaSeriesId SeriesId { get; private set; } = null!;
    public string QuestionText { get; private set; } = string.Empty;

    /// <summary>JSON array of answer option strings, e.g. ["Option A","Option B","Option C"]</summary>
    public string OptionsJson { get; private set; } = string.Empty;

    public string CorrectOption { get; private set; } = string.Empty;
    public int VoteReward { get; private set; }
    public TriviaQuestionStatus Status { get; private set; }
    public int AnswerCount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static TriviaQuestion Create(
        TriviaSeriesId seriesId,
        string questionText,
        string optionsJson,
        string correctOption,
        int voteReward)
    {
        ArgumentNullException.ThrowIfNull(seriesId);
        ArgumentException.ThrowIfNullOrWhiteSpace(questionText);
        ArgumentException.ThrowIfNullOrWhiteSpace(optionsJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(correctOption);
        if (voteReward < 0) throw new DomainException("Vote reward cannot be negative.");

        return new TriviaQuestion
        {
            Id = TriviaQuestionId.Of(Guid.NewGuid()),
            SeriesId = seriesId,
            QuestionText = questionText.Trim(),
            OptionsJson = optionsJson,
            CorrectOption = correctOption.Trim(),
            VoteReward = voteReward,
            Status = TriviaQuestionStatus.Pending,
            AnswerCount = 0,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Publish()
    {
        if (Status != TriviaQuestionStatus.Pending)
            throw new DomainException("Only Pending questions can be published.");
        Status = TriviaQuestionStatus.Active;
    }

    public void Archive()
    {
        if (Status == TriviaQuestionStatus.Archived)
            throw new DomainException("Question is already archived.");
        Status = TriviaQuestionStatus.Archived;
    }

    public void IncrementAnswerCount() => AnswerCount++;
}
