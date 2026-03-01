using Contracts.Contracts;
using MediatR;

namespace Application.Admin.Queries.GetRarityTierConfig;

public record GetRarityTierConfigQuery(Guid OrgId)
    : IRequest<ServiceResponse<List<RarityTierConfigDto>>>;

public record RarityTierConfigDto(
    Guid Id,
    string RarityName,
    int RatingMin,
    int RatingMax,
    int PullWeightBps
);
