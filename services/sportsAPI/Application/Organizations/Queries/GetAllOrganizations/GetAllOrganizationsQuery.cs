using Application.Common.Models;
using Application.Dto.Organization;
using Contracts.Contracts;
using Contracts.Responses;
using MediatR;

namespace Application.Organizations.Queries.GetAllOrganizations;

public record GetAllOrganizationsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? LeagueId = null,
    string? Sport = null,
    string? SortBy = "Name",
    bool SortDescending = false
) : IRequest<ServiceResponse<PaginatedList<OrganizationDto>>>;

