using Application.Cards.Services;
using Application.Dto.Cards;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;

namespace Application.Cards.Commands.CreateCardPlayer;

public sealed class CreateCardPlayerCommandHandler
    : IRequestHandler<CreateCardPlayerCommand, ServiceResponse<CardPlayerDto>>
{
    private readonly IRepository _repo;
    private readonly RarityAssignmentService _rarityService;

    public CreateCardPlayerCommandHandler(IRepository repo, RarityAssignmentService rarityService)
    {
        _repo = repo;
        _rarityService = rarityService;
    }

    public async Task<ServiceResponse<CardPlayerDto>> Handle(
        CreateCardPlayerCommand request,
        CancellationToken cancellationToken)
    {
        var rarityTier = await _rarityService.AssignAsync(
            request.LeagueId, request.OverallRating, cancellationToken);

        var card = CardPlayer.Create(
            request.LeagueId,
            request.Name,
            request.Position,
            request.OverallRating,
            rarityTier);

        await _repo.AddAsync(card, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(ToDto(card), "CardPlayer created successfully.");
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
