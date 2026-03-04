using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Commands.CreateListing;

public record CreateListingCommand(
    Guid UserCardId,
    Guid SellerId,
    long StartingBid,
    long? BuyNowPrice,
    int DurationHours
) : IRequest<ServiceResponse<Guid>>;
