using BuildingBlocks.Exceptions;

namespace Domain.Poll;

/// <summary>
/// Aggregate root for a GM-created fan poll.
/// Owns a collection of PollOption entities.
/// </summary>
public sealed class Poll : Aggregate<PollId>
{
    internal Poll() { } // EF

    public OrganizationId OrganizationId { get; private set; } = null!;
    public string QuestionText { get; private set; } = string.Empty;
    public PollStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<PollOption> _options = new();
    public IReadOnlyList<PollOption> Options => _options;

    public static Poll Create(OrganizationId organizationId, string questionText, IEnumerable<string> optionTexts)
    {
        ArgumentNullException.ThrowIfNull(organizationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(questionText);

        var textList = optionTexts?.ToList() ?? [];
        if (textList.Count < 2) throw new DomainException("A poll must have at least 2 options.");

        var poll = new Poll
        {
            Id = PollId.Of(Guid.NewGuid()),
            OrganizationId = organizationId,
            QuestionText = questionText.Trim(),
            Status = PollStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        for (int i = 0; i < textList.Count; i++)
            poll._options.Add(PollOption.Create(poll.Id, textList[i], i));

        return poll;
    }

    public void Publish()
    {
        if (Status != PollStatus.Draft)
            throw new DomainException("Only Draft polls can be published.");
        Status = PollStatus.Active;
    }

    public void Archive()
    {
        if (Status == PollStatus.Archived)
            throw new DomainException("Poll is already archived.");
        Status = PollStatus.Archived;
    }

    public PollOption GetOption(PollOptionId optionId)
    {
        var option = _options.FirstOrDefault(o => o.Id == optionId)
            ?? throw new DomainException("Option does not belong to this poll.");
        return option;
    }
}
