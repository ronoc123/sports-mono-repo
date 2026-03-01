using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Commands.CreateListing;

public record CreateListingCommand(
    Guid CardOwnerId,
    Guid SellerId,
    Guid OrgId,
    long StartingBid,
    long? BuyNowPrice,
    int DurationHours
) : IRequest<ServiceResponse<Guid>>;
