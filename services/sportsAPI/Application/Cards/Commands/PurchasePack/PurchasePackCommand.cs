using Application.Dto.Cards;
using Contracts.Contracts;
using MediatR;

namespace Application.Cards.Commands.PurchasePack;

public record PurchasePackCommand(
    Guid UserId,
    Guid OrgId,
    Guid LeagueId
) : IRequest<ServiceResponse<PackPurchaseResultDto>>;
