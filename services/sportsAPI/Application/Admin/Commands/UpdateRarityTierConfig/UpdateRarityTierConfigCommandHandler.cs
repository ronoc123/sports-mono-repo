using Application.Admin.Queries.GetRarityTierConfig;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;

namespace Application.Admin.Commands.UpdateRarityTierConfig;

public sealed class UpdateRarityTierConfigCommandHandler
    : IRequestHandler<UpdateRarityTierConfigCommand, ServiceResponse<RarityTierConfigDto>>
{
    private readonly IRepository _repo;

    public UpdateRarityTierConfigCommandHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<RarityTierConfigDto>> Handle(
        UpdateRarityTierConfigCommand request,
        CancellationToken cancellationToken)
    {
        var config = await _repo.GetByIdAsync<RarityTierConfig, Guid>(request.ConfigId, cancellationToken)
            ?? throw new DomainException("Rarity tier config not found.");

        config.Update(request.RatingMin, request.RatingMax, request.PullWeightBps);

        _repo.Update(config);
        await _repo.SaveChangesAsync(cancellationToken);

        var dto = new RarityTierConfigDto(
            config.Id, config.RarityName, config.RatingMin, config.RatingMax, config.PullWeightBps);

        return ServiceResponse.Ok(dto, "Rarity tier updated.");
    }
}
