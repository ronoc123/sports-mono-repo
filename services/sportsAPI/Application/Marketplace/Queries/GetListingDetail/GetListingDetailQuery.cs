using Application.Dto.Marketplace;
using Contracts.Contracts;
using MediatR;

namespace Application.Marketplace.Queries.GetListingDetail;

public record GetListingDetailQuery(Guid ListingId)
    : IRequest<ServiceResponse<ListingDetailDto>>;
