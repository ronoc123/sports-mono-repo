using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Queries.GetAvailableBalance;

public record GetAvailableBalanceQuery(Guid UserId, Guid LeagueId)
    : IRequest<ServiceResponse<long>>;
