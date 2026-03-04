using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Marketplace;

using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Marketplace.Commands.CreateListing;

public sealed class CreateListingCommandHandler
    : IRequestHandler<CreateListingCommand, ServiceResponse<Guid>>
{
    private readonly IRepository _repo;

    public CreateListingCommandHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<Guid>> Handle(
        CreateListingCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify ownership via UserCard
        var userCard = await _repo.Query<UserCard>(asNoTracking: false)
            .FirstOrDefaultAsync(c => c.Id == request.UserCardId, cancellationToken);

        if (userCard is null)
            throw new EntityNotFoundException(nameof(UserCard), request.UserCardId);

        if (userCard.UserId != request.SellerId)
            throw new DomainException("You do not own this card.");

        // 2. Guard: already listed
        if (userCard.IsListed)
            throw new DomainException("Card is already listed.");

        // 3. Create listing aggregate (league-scoped)
        var listing = AuctionListing.Create(
            request.UserCardId,
            request.SellerId,
            userCard.LeagueId,
            request.StartingBid,
            request.BuyNowPrice,
            request.DurationHours);

        // 4. Lock the card
        userCard.MarkAsListed();

        // 5. Persist atomically
        await _repo.AddAsync(listing, cancellationToken);
        _repo.Update(userCard);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(listing.Id, "Listing created successfully.");
    }
}
