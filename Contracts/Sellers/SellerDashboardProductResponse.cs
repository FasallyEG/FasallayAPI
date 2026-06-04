using Fasally.Entities.Enums;

namespace Fasally.Contracts.Sellers;

public record SellerDashboardProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    ProductStatus Status,
    DateTime CreatedAt
);
