using Contracts.Contracts;
using MediatR;

namespace Application.Store.Queries.GetBundleCatalog;

public record GetBundleCatalogQuery(Guid OrganizationId) : IRequest<ServiceResponse<List<ProductDto>>>;
