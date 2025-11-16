using Application.Common.Interfaces;
using Application.Common.Models;
using Application.PlayerOptions.Queries.GetAllPlayerOptions;
using Contracts.Contracts;
using Domain.Organizations.Entities;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Application.PlayerOptions.Commands.CreatePlayerOption;

public class CreatePlayerOptionCommandHandler : IRequestHandler<CreatePlayerOptionCommand, ServiceResponse<Guid>>
{
    private readonly IOrganizationRepository _organizationRepository;
    //private readonly IPlayerRepository _playerRepository;

    public CreatePlayerOptionCommandHandler(
        IOrganizationRepository organizationRepository
        //IPlayerRepository playerRepo
        )
    {
        _organizationRepository = organizationRepository;
        //_playerRepository = playerRepo;
    }

    public async Task<ServiceResponse<Guid>> Handle(CreatePlayerOptionCommand request, CancellationToken cancellationToken)
    {
    //var organization = await _organizationRepository.GetOrganizationByIdAsync(request.OrganizationId, cancellationToken);
    //if (organization is null)
    //{
    //    throw new ValidationException("Organization Not Found.");
    //}

    //var playerId = PlayerId.Of(request.PlayerId);

    //var player = await _playerRepository
    //  .Query(asNoTracking: true)
    //  .Where(p => p.Id == playerId)
    //  .FirstOrDefaultAsync(cancellationToken);

    //if (player is null)
    //{
    //    throw new ValidationException("Player Not Found.");
    //}

    //if (player.LeagueId != organization.LeagueId)
    //{
    //  throw new ValidationException("Player must be in the same league as the organization");
    //}

    //var option = organization.CreatePlayerOption(
    //  request.Title,
    //  request.Description,
    //  playerId,
    //  request.ExpiresAt);

    //await _organizationRepository.UpdateOrganizationAsync(organization, cancellationToken);
    //await _organizationRepository.SaveChangesAsync(cancellationToken);

    //return ServiceResponse.Ok(option.Id.Value, "Player option created.");

    return null;

  }
}
