using BuildingBlocks.Exceptions;

namespace Domain.Trivia;

/// <summary>
/// Records a fan's answer to a trivia question.
/// Idempotency enforced by UNIQUE (UserId, TriviaQuestionId) at the DB level.
/// </summary>
public sealed class TriviaAnswer
{
    internal TriviaAnswer() { } // EF

    public TriviaAnswerId Id { get; private set; } = null!;
    public TriviaQuestionId TriviaQuestionId { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public string SelectedOption { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    public static TriviaAnswer Record(
        TriviaQuestionId questionId,
        Guid userId,
        string selectedOption,
        bool isCorrect)
    {
        ArgumentNullException.ThrowIfNull(questionId);
        if (userId == Guid.Empty) throw new DomainException("UserId cannot be empty.");
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedOption);

        return new TriviaAnswer
        {
            Id = TriviaAnswerId.Of(Guid.NewGuid()),
            TriviaQuestionId = questionId,
            UserId = userId,
            SelectedOption = selectedOption,
            IsCorrect = isCorrect,
            AnsweredAt = DateTime.UtcNow
        };
    }
}
