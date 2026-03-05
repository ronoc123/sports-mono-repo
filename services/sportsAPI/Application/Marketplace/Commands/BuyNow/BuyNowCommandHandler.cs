using Application.Common.Interfaces;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Marketplace;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Marketplace.Commands.BuyNow;

public sealed class BuyNowCommandHandler
    : IRequestHandler<BuyNowCommand, ServiceResponse<bool>>
{
    private readonly IRepository _repo;
    private readonly IBalanceNotificationService _balanceNotifier;

    public BuyNowCommandHandler(IRepository repo, IBalanceNotificationService balanceNotifier)
    {
        _repo = repo;
        _balanceNotifier = balanceNotifier;
    }

    public async Task<ServiceResponse<bool>> Handle(
        BuyNowCommand request,
        CancellationToken cancellationToken)
    {
        var listingId = request.ListingId.ToString();
        var leagueId = LeagueId.Of(request.LeagueId);

        // 1. Load listing (tracked)
        var listing = await _repo.Query<AuctionListing>(asNoTracking: false)
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(AuctionListing), request.ListingId);

        // 2. Guards
        if (!listing.IsActive)
            throw new DomainException("Auction has expired or is no longer active.");

        if (!listing.BuyNowPrice.HasValue)
            throw new DomainException("This listing does not have a buy now price.");

        if (listing.SellerId == request.BuyerId)
            throw new DomainException("Cannot buy your own listing.");

        var buyNowPrice = listing.BuyNowPrice.Value;

        // 3. Load buyer's account (tracked)
        var buyerAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            leagueId,
            UserId.Of(request.BuyerId))
            ?? throw new DomainException("Vote account not found for buyer.");

        if (buyerAccount.Balance < buyNowPrice)
            throw new DomainException("Insufficient balance.");

        // 4. Load seller's account (tracked)
        var sellerAccount = await _repo.GetByIdAsync<VoteAccount>(
            cancellationToken,
            leagueId,
            UserId.Of(listing.SellerId))
            ?? throw new DomainException("Vote account not found for seller.");

        // 5. Load user card (tracked) for ownership transfer
        var userCard = await _repo.Query<UserCard>(asNoTracking: false)
            .FirstOrDefaultAsync(uc => uc.Id == listing.UserCardId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(UserCard), listing.UserCardId);

        // 6. Release all active escrows for this listing (refund all existing bidders)
        var activeEscrows = await _repo.Query<PointsEscrow>(asNoTracking: false)
            .Where(e => e.ListingId == request.ListingId && e.Status == "held")
            .ToListAsync(cancellationToken);

        foreach (var escrow in activeEscrows)
        {
            var bidderAccount = await _repo.GetByIdAsync<VoteAccount>(
                cancellationToken,
                leagueId,
                UserId.Of(escrow.UserId))
                ?? throw new DomainException($"Vote account not found for bidder {escrow.UserId}.");

            escrow.Release();
            bidderAccount.CreditBidRelease(escrow.HeldAmount, listingId);
            _repo.Update(escrow);
            _repo.Update(bidderAccount);

            _ = _balanceNotifier.NotifyBalanceChangedAsync(escrow.UserId.ToString(), bidderAccount.Balance, cancellationToken);
        }

        // 7. Deduct from buyer and credit seller
        buyerAccount.DeductForBuyNow(buyNowPrice, listingId);
        sellerAccount.CreditAuctionSale(buyNowPrice, listingId);

        // 8. Transfer card ownership and settle listing
        userCard.TransferTo(request.BuyerId);
        listing.MarkAsSold();

        // 9. Persist all atomically
        _repo.Update(buyerAccount);
        _repo.Update(sellerAccount);
        _repo.Update(userCard);
        _repo.Update(listing);
        await _repo.SaveChangesAsync(cancellationToken);

        _ = _balanceNotifier.NotifyBalanceChangedAsync(request.BuyerId.ToString(), buyerAccount.Balance, cancellationToken);
        _ = _balanceNotifier.NotifyBalanceChangedAsync(listing.SellerId.ToString(), sellerAccount.Balance, cancellationToken);

        return ServiceResponse.Ok(true, "Buy now purchase completed. Card transferred to your collection.");
    }
}
