using Contracts.Contracts;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;

namespace Application.Poll.Commands.CreatePoll;

public sealed class CreatePollCommandHandler
    : IRequestHandler<CreatePollCommand, ServiceResponse<CreatePollResult>>
{
    private readonly IPollRepository _repo;

    public CreatePollCommandHandler(IPollRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<CreatePollResult>> Handle(
        CreatePollCommand request, CancellationToken ct)
    {
        var poll = Domain.Poll.Poll.Create(
            OrganizationId.Of(request.OrganizationId),
            request.QuestionText,
            request.Options);

        poll.Publish(); // Story 4.1 AC: poll is created Active

        await _repo.AddAsync(poll, ct);
        await _repo.SaveChangesAsync(ct);

        var optionIds = poll.Options.Select(o => o.Id.Value).ToList();

        return ServiceResponse.Ok(
            new CreatePollResult(poll.Id.Value, optionIds),
            "Poll created.");
    }
}
