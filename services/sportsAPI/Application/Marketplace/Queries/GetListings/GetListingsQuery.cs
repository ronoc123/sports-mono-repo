using Application.Dto.Marketplace;
using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Queries.GetListings;

public record GetListingsQuery(
    Guid LeagueId,
    int Page = 1,
    int PageSize = 20
) : IRequest<ServiceResponse<List<ListingDto>>>;
