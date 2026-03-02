using Application.Cards.Services;
using Application.Dto.Cards;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;

namespace Application.Cards.Commands.UpdateCardPlayer;

public sealed class UpdateCardPlayerCommandHandler
    : IRequestHandler<UpdateCardPlayerCommand, ServiceResponse<CardPlayerDto>>
{
    private readonly IRepository _repo;
    private readonly RarityAssignmentService _rarityService;

    public UpdateCardPlayerCommandHandler(IRepository repo, RarityAssignmentService rarityService)
    {
        _repo = repo;
        _rarityService = rarityService;
    }

    public async Task<ServiceResponse<CardPlayerDto>> Handle(
        UpdateCardPlayerCommand request,
        CancellationToken cancellationToken)
    {
        var card = await _repo.GetByIdAsync<CardPlayer, Guid>(
            request.CardPlayerId, cancellationToken);

        if (card is null)
            throw new DomainException("CardPlayer not found.");

        // Enforce GM league ownership: a GM can only edit cards in their own league
        if (card.LeagueId != request.LeagueId)
            throw new UnauthorizedAccessException(
                "You are not authorized to edit CardPlayers in another league.");

        var rarityTier = await _rarityService.AssignAsync(
            request.LeagueId, request.OverallRating, cancellationToken);

        card.Update(request.Name, request.Position, request.OverallRating, rarityTier);

        _repo.Update(card);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(ToDto(card), "CardPlayer updated successfully.");
    }

    private static CardPlayerDto ToDto(CardPlayer c) => new()
    {
        Id = c.Id,
        LeagueId = c.LeagueId,
        Name = c.Name,
        Position = c.Position,
        OverallRating = c.OverallRating,
        RarityTier = c.RarityTier,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.LastModified,
    };
}
