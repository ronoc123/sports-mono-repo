using Application.Common.Models;
using Contracts.Contracts;
using Domain.Player;
using Domain.PlayerOption;
using Domain.Organizations;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.PlayerOptions.Commands.CreatePlayerOption
{
  public class CreatePlayerOptionCommandHandler
      : IRequestHandler<CreatePlayerOptionCommand, ServiceResponse<Guid>>
  {
    private readonly IRepository _repo;

    public CreatePlayerOptionCommandHandler(IRepository repo)
    {
      _repo = repo;
    }

    public async Task<ServiceResponse<Guid>> Handle(
        CreatePlayerOptionCommand request,
        CancellationToken cancellationToken)
    {

      var organization = await _repo.GetByIdAsync<Organization, OrganizationId>(request.OrganizationId, cancellationToken);

      if (organization is null)
        throw new ValidationException("Organization not found.");

      var playerId = request.PlayerId;

      var player = await _repo
          .Query<Player>(asNoTracking: true)
          .Where(p => p.Id == playerId)
          .FirstOrDefaultAsync(cancellationToken);

      if (player is null)
        throw new ValidationException("Player not found.");

      if (player.LeagueId != organization.LeagueId)
        throw new ValidationException("Player must be in the same league as the organization.");

      var option = PlayerOption.Create(request.Title, request.Description, playerId, organization.Id, request.ExpiresAt);

      await _repo.AddAsync(option, cancellationToken);
      await _repo.SaveChangesAsync(cancellationToken);

      return ServiceResponse.Ok(option.Id.Value, "Player option created.");
    }
  }
}
