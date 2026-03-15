using Application.Dto;
using Contracts.Contracts;
using Domain.AIConversation;
using MediatR;

namespace Application.AIConversation.Queries.GetAISession;

public sealed class GetAISessionQueryHandler
    : IRequestHandler<GetAISessionQuery, ServiceResponse<AISessionDto?>>
{
    private readonly IAISessionRepository _repo;

    public GetAISessionQueryHandler(IAISessionRepository repo) => _repo = repo;

    public async Task<ServiceResponse<AISessionDto?>> Handle(
        GetAISessionQuery request, CancellationToken cancellationToken)
    {
        var session = await _repo.GetByLinkedEntityAsync(request.LinkedEntityId, cancellationToken);

        if (session is null)
            return ServiceResponse.Ok<AISessionDto?>(null, "No AI session found.");

        return ServiceResponse.Ok<AISessionDto?>(new AISessionDto
        {
            Id = session.Id.Value,
            UserId = session.UserId,
            Context = session.Context,
            LinkedEntityId = session.LinkedEntityId,
            Messages = session.Messages.Select(m => new AIMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                Timestamp = m.Timestamp
            }).ToList()
        }, "Success");
    }
}
