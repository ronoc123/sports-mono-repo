using BuildingBlocks.Exceptions;

namespace Domain.Trivia;

/// <summary>
/// Aggregate root for a GM-created trivia series.
/// Owns a collection of TriviaQuestion entities.
/// </summary>
public sealed class TriviaSeries : Aggregate<TriviaSeriesId>
{
    internal TriviaSeries() { } // EF

    public OrganizationId OrganizationId { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private readonly List<TriviaQuestion> _questions = new();
    public IReadOnlyList<TriviaQuestion> Questions => _questions;

    public static TriviaSeries Create(OrganizationId organizationId, string name)
    {
        ArgumentNullException.ThrowIfNull(organizationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.Trim().Length > 200) throw new DomainException("Series name cannot exceed 200 characters.");

        return new TriviaSeries
        {
            Id = TriviaSeriesId.Of(Guid.NewGuid()),
            OrganizationId = organizationId,
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public TriviaQuestion AddQuestion(
        string questionText,
        string optionsJson,
        string correctOption,
        int voteReward)
    {
        var question = TriviaQuestion.Create(Id, questionText, optionsJson, correctOption, voteReward);
        _questions.Add(question);
        return question;
    }
}
