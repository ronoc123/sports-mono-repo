using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Commands.PlaceBid;

public record PlaceBidCommand(
    Guid ListingId,
    Guid BidderId,
    Guid OrgId,
    long Amount
) : IRequest<ServiceResponse<bool>>;
