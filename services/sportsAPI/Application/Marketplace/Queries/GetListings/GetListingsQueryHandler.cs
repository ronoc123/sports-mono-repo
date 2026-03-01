using Application.Dto.Marketplace;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Marketplace;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Marketplace.Queries.GetListings;

public sealed class GetListingsQueryHandler
    : IRequestHandler<GetListingsQuery, ServiceResponse<List<ListingDto>>>
{
    private readonly IRepository _repo;

    public GetListingsQueryHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<List<ListingDto>>> Handle(
        GetListingsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Listings join with CardOwner and CardPlayer for display data
        var listings = await _repo.Query<AuctionListing>()
            .Where(l => l.OrgId == request.OrgId
                     && l.Status == "active"
                     && l.ExpiresAt > now)
            .OrderBy(l => l.ExpiresAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var listingIds = listings.Select(l => l.Id).ToList();
        var cardOwnerIds = listings.Select(l => l.CardOwnerId).ToList();

        // Load bid counts
        var bidCounts = await _repo.Query<Bid>()
            .Where(b => listingIds.Contains(b.ListingId))
            .GroupBy(b => b.ListingId)
            .Select(g => new { ListingId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ListingId, x => x.Count, cancellationToken);

        // Load card owner + player data
        var cardOwners = await _repo.Query<CardOwner>()
            .Include(co => co.CardPlayer)
            .Where(co => cardOwnerIds.Contains(co.Id))
            .ToDictionaryAsync(co => co.Id, cancellationToken);

        var dtos = listings.Select(l =>
        {
            cardOwners.TryGetValue(l.CardOwnerId, out var co);
            var cp = co?.CardPlayer;
            return new ListingDto
            {
                Id = l.Id,
                CardOwnerId = l.CardOwnerId,
                SellerId = l.SellerId,
                CardName = cp?.Name ?? string.Empty,
                Position = cp?.Position ?? string.Empty,
                OverallRating = cp?.OverallRating ?? 0,
                RarityTier = cp?.RarityTier ?? string.Empty,
                StartingBid = l.StartingBid,
                BuyNowPrice = l.BuyNowPrice,
                CurrentBid = l.CurrentBid,
                Status = l.Status,
                ExpiresAt = l.ExpiresAt,
                CreatedAt = l.CreatedAt ?? DateTime.UtcNow,
                BidCount = bidCounts.TryGetValue(l.Id, out var cnt) ? cnt : 0,
            };
        }).ToList();

        return ServiceResponse.Ok(dtos);
    }
}
