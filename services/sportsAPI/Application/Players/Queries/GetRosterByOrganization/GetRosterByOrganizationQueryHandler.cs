using Application.Dto.Player;
using Contracts.Contracts;
using Domain.Player;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Players.Queries.GetRosterByOrganization;

public class GetRosterByOrganizationQueryHandler
    : IRequestHandler<GetRosterByOrganizationQuery, ServiceResponse<List<PlayerDto>>>
{
    private readonly IRepository _repo;

    public GetRosterByOrganizationQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<PlayerDto>>> Handle(
        GetRosterByOrganizationQuery request,
        CancellationToken cancellationToken)
    {
        var orgId = OrganizationId.Of(request.OrganizationId);

        var list = await _repo.Query<Player>()
            .Where(p => p.OrganizationId == orgId)
            .OrderBy(p => p.Name)
            .Select(p => new PlayerDto
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
            })
            .ToListAsync(cancellationToken);

        return ServiceResponse.Ok(list, "Success");
    }
}
