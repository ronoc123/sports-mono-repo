using Application.Admin.Queries.GetRarityTierConfig;
using Contracts.Contracts;
using MediatR;

namespace Application.Admin.Commands.UpdateRarityTierConfig;

public record UpdateRarityTierConfigCommand(
    Guid ConfigId,
    int RatingMin,
    int RatingMax,
    int PullWeightBps
) : IRequest<ServiceResponse<RarityTierConfigDto>>;
