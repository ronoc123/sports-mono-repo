using Contracts.Contracts;
using Domain.Enums;
using Domain.Product;
using Domain.Repositories;
using MediatR;

namespace Application.Store.Queries.GetBundleCatalog;

public sealed class GetBundleCatalogQueryHandler
    : IRequestHandler<GetBundleCatalogQuery, ServiceResponse<List<ProductDto>>>
{
    private readonly IRepository _repo;

    public GetBundleCatalogQueryHandler(IRepository repo)
    {
        _repo = repo;
    }

    public async Task<ServiceResponse<List<ProductDto>>> Handle(
        GetBundleCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _repo.ListAsync<Product>(
            filter: p => p.IsActive && p.Type == ProductType.Votes,
            orderBy: q => q.OrderBy(p => p.Price.Amount),
            ct: cancellationToken);

        var dtos = products
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Quantity,
                p.Price.Amount,
                p.Price.Currency))
            .ToList();

        return ServiceResponse.Ok(dtos, "Bundle catalog retrieved.");
    }
}
