using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Commands.BuyNow;

public record BuyNowCommand(
    Guid ListingId,
    Guid BuyerId,
    Guid OrgId
) : IRequest<ServiceResponse<bool>>;
