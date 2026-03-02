using Application.Dto.Cards;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cards.Queries.GetCollection;

public sealed class GetCollectionQueryHandler
    : IRequestHandler<GetCollectionQuery, ServiceResponse<List<UserCardDto>>>
{
    private readonly IRepository _repo;

    public GetCollectionQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<UserCardDto>>> Handle(GetCollectionQuery request, CancellationToken cancellationToken)
    {
        var cards = await _repo.Query<UserCard>()
            .Where(u => u.UserId == request.UserId && u.LeagueId == request.LeagueId)
            .Include(u => u.CardPlayer)
            .Select(u => new UserCardDto
            {
                Id = u.Id,
                CardPackId = u.CardPackId,
                CardPlayerId = u.CardPlayerId,
                LeagueId = u.LeagueId,
                Name = u.CardPlayer != null ? u.CardPlayer.Name : string.Empty,
                Position = u.CardPlayer != null ? u.CardPlayer.Position : string.Empty,
                OverallRating = u.CardPlayer != null ? u.CardPlayer.OverallRating : 0,
                RarityTier = u.RarityTier,
                IsListed = u.IsListed,
                PulledAt = u.CreatedAt,
            })
            .OrderByDescending(u => u.PulledAt)
            .ToListAsync(cancellationToken);

        return ServiceResponse.Ok(cards, "Success");
    }
}
