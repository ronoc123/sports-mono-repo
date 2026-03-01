using Application.Dto.Cards;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cards.Queries.GetCardPlayers;

public sealed class GetCardPlayersQueryHandler
    : IRequestHandler<GetCardPlayersQuery, ServiceResponse<List<CardPlayerDto>>>
{
    private readonly IRepository _repo;

    public GetCardPlayersQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<CardPlayerDto>>> Handle(
        GetCardPlayersQuery request,
        CancellationToken cancellationToken)
    {
        var list = await _repo.Query<CardPlayer>()
            .Where(c => c.LeagueId == request.LeagueId)
            .Select(c => new CardPlayerDto
            {
                Id = c.Id,
                LeagueId = c.LeagueId,
                Name = c.Name,
                Position = c.Position,
                OverallRating = c.OverallRating,
                RarityTier = c.RarityTier,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.LastModified,
            })
            .ToListAsync(cancellationToken);

        return ServiceResponse.Ok(list, "Success");
    }
}
