namespace Fasally.Contracts.Products;

public record ProductVariantResponse(
    Guid Id,
    string Type,
    string Value
);
