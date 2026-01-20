using Application.Dto.PlayerOption;
using Contracts.Contracts;
using MediatR;

namespace Application.PlayerOptions.Queries.GetAllPlayerOptions;

public record GetAllPlayerOptionsQuery(
    Guid? OrganizationId = null,
    Guid? PlayerId = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    bool? IsActive = null,
    bool? IsExpired = null,
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<ServiceResponse<List<PlayerOptionDto>>>;

