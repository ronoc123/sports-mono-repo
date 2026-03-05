using Application.Common.Interfaces;
using Application.Marketplace.Services;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Marketplace;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Marketplace.Commands.PlaceBid;

public sealed class PlaceBidCommandHandler
    : IRequestHandler<PlaceBidCommand, ServiceResponse<bool>>
{
    /// <summary>New bid must exceed current bid by at least this fraction.</summary>
    private const decimal MinBidIncrement = 0.05m;

    private readonly IRepository _repo;
    private readonly IAuctionSignalRService _signalR;
    private readonly IBalanceNotificationService _balanceNotifier;

    public PlaceBidCommandHandler(IRepository repo, IAuctionSignalRService signalR, IBalanceNotificationService balanceNotifier)
    {
        _repo = repo;
        _signalR = signalR;
        _balanceNotifier = balanceNotifier;
    }

    public async Task<ServiceResponse<bool>> Handle(
        PlaceBidCommand request,
        CancellationToken cancellationToken)
    {
        var listingId = request.ListingId.ToString();
        var leagueId = LeagueId.Of(request.LeagueId);

        // 1. Load listing (tracked — will be updated)
        var listing = await _repo.Query<AuctionListing>(asNoTracking: false)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(AuctionListing), request.ListingId);

        // 2. Guard: listing must be active and not expired
        if (!listing.IsActive)
            throw new DomainException("Auction has expired or is no longer active.");

        // 3. Guard: no self-bid
        if (listing.SellerId == request.BidderId)
            throw new DomainException("Cannot bid on your own listing.");

        // 4. Guard: minimum bid increment (must beat current bid by ≥ 5%)
        var minBid = (long)Math.Ceiling(listing.CurrentBid * (1 + MinBidIncrement));
        if (request.Amount < minBid)
            throw new DomainException(
                $"Bid must exceed current bid by at least 5%. Minimum bid: {minBid} points.");

        // 5. Load bidder's VoteAccount (tracked)
        var bidderAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            leagueId,
            UserId.Of(request.BidderId))
            ?? throw new DomainException("Vote account not found for bidder.");

        // 6. Guard: sufficient balance (Balance already reflects prior escrow deductions)
        if (bidderAccount.Balance < request.Amount)
            throw new DomainException("Insufficient balance.");

        // 7. Find previous high bidder's active escrow for this listing (if any)
        var previousEscrow = await _repo.Query<PointsEscrow>(asNoTracking: false)
            .FirstOrDefaultAsync(
                e => e.ListingId == request.ListingId && e.Status == "held",
                cancellationToken);

        // 8. Release previous high bidder's escrow + credit their account
        if (previousEscrow is not null)
        {
            var previousBidderAccount = await _repo.GetByIdAsync<VoteAccount>(
                cancellationToken,
                leagueId,
                UserId.Of(previousEscrow.UserId))
                ?? throw new DomainException("Vote account not found for previous bidder.");

            previousEscrow.Release();
            previousBidderAccount.CreditBidRelease(previousEscrow.HeldAmount, listingId);
            _repo.Update(previousEscrow);
            _repo.Update(previousBidderAccount);

            _ = _balanceNotifier.NotifyBalanceChangedAsync(previousEscrow.UserId.ToString(), previousBidderAccount.Balance, cancellationToken);
        }

        // 9. Create new escrow for the incoming bid
        var newEscrow = PointsEscrow.Create(request.BidderId, request.ListingId, request.Amount);

        // 10. Deduct from bidder's balance
        bidderAccount.DeductForBidEscrow(request.Amount, listingId);

        // 11. Record bid and update listing's current bid
        var bid = Bid.Create(request.ListingId, request.BidderId, request.Amount);
        listing.UpdateCurrentBid(request.Amount);

        // 12. Persist all — single SaveChanges = implicit transaction
        await _repo.AddAsync(newEscrow, cancellationToken);
        await _repo.AddAsync(bid, cancellationToken);
        _repo.Update(bidderAccount);
        _repo.Update(listing);
        await _repo.SaveChangesAsync(cancellationToken);

        // 13. Emit real-time events (fire-and-forget — do not fail the command on SignalR error)
        var broadcastTs = DateTime.UtcNow;

        _ = _balanceNotifier.NotifyBalanceChangedAsync(request.BidderId.ToString(), bidderAccount.Balance, cancellationToken);

        // Outbid notification to the previous high bidder
        if (previousEscrow is not null)
        {
            _ = _signalR.SendOutbidNotificationAsync(
                previousEscrow.UserId,
                request.ListingId,
                previousEscrow.HeldAmount,
                cancellationToken);
        }

        // Live bid update to all viewers of this listing
        _ = _signalR.BroadcastBidPlacedAsync(
            request.ListingId,
            listing.CurrentBid,
            request.BidderId,
            broadcastTs,
            cancellationToken);

        return ServiceResponse.Ok(true, "Bid placed successfully.");
    }
}
