using Application.Dto.Marketplace;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Marketplace;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Marketplace.Queries.GetListingDetail;

public sealed class GetListingDetailQueryHandler
    : IRequestHandler<GetListingDetailQuery, ServiceResponse<ListingDetailDto>>
{
    private readonly IRepository _repo;

    public GetListingDetailQueryHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<ListingDetailDto>> Handle(
        GetListingDetailQuery request,
        CancellationToken cancellationToken)
    {
        var listing = await _repo.Query<AuctionListing>()
            .FirstOrDefaultAsync(l => l.Id == request.ListingId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(AuctionListing), request.ListingId);

        var userCard = await _repo.Query<UserCard>()
            .Include(uc => uc.CardPlayer)
            .FirstOrDefaultAsync(uc => uc.Id == listing.UserCardId, cancellationToken);

        var cp = userCard?.CardPlayer;

        var bids = await _repo.Query<Bid>()
            .Where(b => b.ListingId == listing.Id)
            .OrderByDescending(b => b.Timestamp)
            .Select(b => new BidDto
            {
                Id = b.Id,
                BidderId = b.BidderId,
                Amount = b.Amount,
                Timestamp = b.Timestamp,
            })
            .ToListAsync(cancellationToken);

        var dto = new ListingDetailDto
        {
            Id = listing.Id,
            UserCardId = listing.UserCardId,
            SellerId = listing.SellerId,
            CardName = cp?.Name ?? string.Empty,
            Position = cp?.Position ?? string.Empty,
            OverallRating = cp?.OverallRating ?? 0,
            RarityTier = userCard?.RarityTier ?? string.Empty,
            StartingBid = listing.StartingBid,
            BuyNowPrice = listing.BuyNowPrice,
            CurrentBid = listing.CurrentBid,
            Status = listing.Status,
            ExpiresAt = listing.ExpiresAt,
            CreatedAt = listing.CreatedAt ?? DateTime.UtcNow,
            BidCount = bids.Count,
            BidHistory = bids,
        };

        return ServiceResponse.Ok(dto);
    }
}
