using Fasally.Entities.Enums;

namespace Fasally.Contracts.Products;

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    int? CategoryId,
    ProductStatus Status = ProductStatus.Active
);
