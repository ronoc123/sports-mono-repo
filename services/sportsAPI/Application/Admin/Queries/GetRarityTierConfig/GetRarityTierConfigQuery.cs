using Contracts.Contracts;
using MediatR;

namespace Application.Admin.Queries.GetRarityTierConfig;

public record GetRarityTierConfigQuery(Guid LeagueId)
    : IRequest<ServiceResponse<List<RarityTierConfigDto>>>;

public record RarityTierConfigDto(
    Guid Id,
    string RarityName,
    int RatingMin,
    int RatingMax,
    int PullWeightBps
);
