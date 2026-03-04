using Application.Marketplace.Services;
using Domain.Cards;
using Domain.Marketplace;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Marketplace.Commands.SettleAuction;

public sealed class SettleAuctionCommandHandler : IRequestHandler<SettleAuctionCommand>
{
    private readonly IRepository _repo;
    private readonly IAuctionSignalRService _signalR;
    private readonly ILogger<SettleAuctionCommandHandler> _logger;

    public SettleAuctionCommandHandler(
        IRepository repo,
        IAuctionSignalRService signalR,
        ILogger<SettleAuctionCommandHandler> logger)
    {
        _repo = repo;
        _signalR = signalR;
        _logger = logger;
    }

    public async Task Handle(SettleAuctionCommand request, CancellationToken cancellationToken)
    {
        // Load listing (tracked — will be mutated)
        var listing = await _repo.Query<AuctionListing>(asNoTracking: false)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken);

        if (listing is null)
        {
            _logger.LogWarning("SettleAuctionCommand: listing {ListingId} not found.", request.ListingId);
            return;
        }

        // Idempotent guard — already settled/expired/sold
        if (listing.Status != "active")
        {
            _logger.LogDebug("SettleAuctionCommand: listing {ListingId} already in status '{Status}'. No-op.",
                request.ListingId, listing.Status);
            return;
        }

        var listingId = listing.Id.ToString();

        // Load the user card (tracked) — always unlisted on settlement
        var userCard = await _repo.Query<UserCard>(asNoTracking: false)
            .FirstOrDefaultAsync(uc => uc.Id == listing.UserCardId, cancellationToken);

        // Find the winning escrow — the one PointsEscrow still held for this listing
        var winningEscrow = await _repo.Query<PointsEscrow>(asNoTracking: false)
            .FirstOrDefaultAsync(
                e => e.ListingId == listing.Id && e.Status == "held",
                cancellationToken);

        if (winningEscrow is null)
        {
            // ── No bids: expire the listing ───────────────────────────────
            listing.MarkAsExpired();
            userCard?.MarkAsUnlisted();

            if (userCard is not null) _repo.Update(userCard);
            _repo.Update(listing);
            await _repo.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "SettleAuctionCommand: listing {ListingId} expired with no bids.", request.ListingId);

            _ = _signalR.BroadcastAuctionSettledAsync(listing.Id, winnerId: null, finalBid: 0, cancellationToken);
            return;
        }

        // ── Has a winner: settle the listing ──────────────────────────────

        // OrgId comes from the UserCard (VoteAccount is org-scoped)
        var orgId = userCard?.OrgId ?? Guid.Empty;
        if (orgId == Guid.Empty)
        {
            _logger.LogError(
                "SettleAuctionCommand: could not resolve OrgId for listing {ListingId}. Aborting.",
                request.ListingId);
            return;
        }

        // Load seller VoteAccount (tracked) — receives the winning bid
        var sellerAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            OrganizationId.Of(orgId),
            UserId.Of(listing.SellerId));

        if (sellerAccount is null)
        {
            _logger.LogError(
                "SettleAuctionCommand: seller VoteAccount not found for listing {ListingId}. Aborting.",
                request.ListingId);
            return;
        }

        // Settle the winning escrow (marks it consumed) and credit seller
        winningEscrow.Settle();
        sellerAccount.CreditAuctionSale(winningEscrow.HeldAmount, listingId);

        // Release any other lingering held escrows (defensive — shouldn't exist normally)
        var otherEscrows = await _repo.Query<PointsEscrow>(asNoTracking: false)
            .Where(e => e.ListingId == listing.Id && e.Status == "held" && e.Id != winningEscrow.Id)
            .ToListAsync(cancellationToken);

        foreach (var staleEscrow in otherEscrows)
        {
            var staleAccount = await _repo.GetByIdAsync<VoteAccount>(
                cancellationToken,
                OrganizationId.Of(orgId),
                UserId.Of(staleEscrow.UserId));

            if (staleAccount is not null)
            {
                staleEscrow.Release();
                staleAccount.CreditBidRelease(staleEscrow.HeldAmount, listingId);
                _repo.Update(staleAccount);
            }

            _repo.Update(staleEscrow);
        }

        // Transfer card ownership to winner and mark listing settled
        userCard?.TransferTo(winningEscrow.UserId);
        listing.MarkAsSettled();

        // Persist all — single SaveChanges = implicit transaction
        _repo.Update(winningEscrow);
        _repo.Update(sellerAccount);
        if (userCard is not null) _repo.Update(userCard);
        _repo.Update(listing);
        await _repo.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "SettleAuctionCommand: listing {ListingId} settled. Winner={WinnerId}, Amount={Amount}.",
            request.ListingId, winningEscrow.UserId, winningEscrow.HeldAmount);

        _ = _signalR.BroadcastAuctionSettledAsync(
            listing.Id,
            winningEscrow.UserId,
            winningEscrow.HeldAmount,
            cancellationToken);
    }
}
