using Application.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.AIConversation.Queries.GetAISession;

public record GetAISessionQuery(Guid LinkedEntityId) : IRequest<ServiceResponse<AISessionDto?>>;
