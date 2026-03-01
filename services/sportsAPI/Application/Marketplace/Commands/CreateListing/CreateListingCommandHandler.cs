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
        // 1. Verify ownership
        var cardOwner = await _repo.Query<CardOwner>(asNoTracking: false)
            .FirstOrDefaultAsync(c => c.Id == request.CardOwnerId, cancellationToken);

        if (cardOwner is null)
            throw new EntityNotFoundException(nameof(CardOwner), request.CardOwnerId);

        if (cardOwner.UserId != request.SellerId)
            throw new DomainException("You do not own this card.");

        // 2. Guard: already listed
        if (cardOwner.IsListed)
            throw new DomainException("Card is already listed.");

        // 3. Create listing aggregate
        var listing = AuctionListing.Create(
            request.CardOwnerId,
            request.SellerId,
            request.OrgId,
            request.StartingBid,
            request.BuyNowPrice,
            request.DurationHours);

        // 4. Lock the card
        cardOwner.MarkAsListed();

        // 5. Persist atomically
        await _repo.AddAsync(listing, cancellationToken);
        _repo.Update(cardOwner);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(listing.Id, "Listing created successfully.");
    }
}
