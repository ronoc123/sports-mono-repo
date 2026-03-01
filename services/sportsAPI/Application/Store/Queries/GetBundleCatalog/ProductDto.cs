namespace Application.Store.Queries.GetBundleCatalog;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    int Quantity,
    decimal PriceAmount,
    string PriceCurrency
);
