using Application.Dto.Player;
using Contracts.Contracts;
using Domain.Player;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Players.Queries.GetAllPlayers;


public class GetAllPlayersQueryHandler : IRequestHandler<GetAllPlayersQuery, ServiceResponse<List<PlayerDto>>>
{
    private readonly IRepository _repo;

    public GetAllPlayersQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<PlayerDto>>> Handle(GetAllPlayersQuery request, CancellationToken cancellationToken)
    {
        var query = _repo.Query<Player>().Where(p => p.LeagueId == LeagueId.Of(request.LeagueId));

        var dtoQuery = query.Select(p => new PlayerDto
        {
            Id = p.Id.Value,
            Name = p.Name,
            Position = p.Position,
            ImageUrl = p.ImageUrl,
            Age = p.Age,
            LeagueId = p.LeagueId.Value,
            OrganizationId = p.OrganizationId != null ? p.OrganizationId.Value : null,
            IsActive = p.Age >= 16 && p.Age <= 50,
            IsVeteran = p.Age >= 35,
            IsYoungPlayer = p.Age <= 23,
        });

        var list = await dtoQuery.ToListAsync(cancellationToken);

        return ServiceResponse.Ok(list, "Success");
    }
}
