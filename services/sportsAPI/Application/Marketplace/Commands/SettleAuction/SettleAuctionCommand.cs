using MediatR;

namespace Application.Marketplace.Commands.SettleAuction;

/// <summary>
/// Settles a single expired AuctionListing.
/// Dispatched by AuctionExpiryService for every listing where ExpiresAt &lt; UtcNow.
/// Idempotent: if the listing is already settled/expired the handler is a no-op.
/// </summary>
public record SettleAuctionCommand(Guid ListingId) : IRequest;
