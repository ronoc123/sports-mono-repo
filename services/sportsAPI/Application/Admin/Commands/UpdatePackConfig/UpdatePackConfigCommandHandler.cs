using Application.Admin.Queries.GetPackConfig;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.Cards;
using Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Commands.UpdatePackConfig;

public sealed class UpdatePackConfigCommandHandler
    : IRequestHandler<UpdatePackConfigCommand, ServiceResponse<PackConfigDto>>
{
    private readonly IRepository _repo;

    public UpdatePackConfigCommandHandler(IRepository repo) => _repo = repo;

    public async Task<ServiceResponse<PackConfigDto>> Handle(
        UpdatePackConfigCommand request,
        CancellationToken cancellationToken)
    {
        if (request.PackPointCost <= 0)
            throw new DomainException("Pack point cost must be positive.");

        var existing = await _repo.Query<PackConfig>(asNoTracking: false)
            .FirstOrDefaultAsync(p => p.OrgId == request.OrgId, cancellationToken);

        if (existing is null)
        {
            var created = PackConfig.Create(request.OrgId, request.PackPointCost);
            await _repo.AddAsync(created, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);
            return ServiceResponse.Ok(new PackConfigDto(created.OrgId, created.PackPointCost), "Pack config created.");
        }

        existing.UpdateCost(request.PackPointCost);
        _repo.Update(existing);
        await _repo.SaveChangesAsync(cancellationToken);

        return ServiceResponse.Ok(new PackConfigDto(existing.OrgId, existing.PackPointCost), "Pack config updated.");
    }
}
