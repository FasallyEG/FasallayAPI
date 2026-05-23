using Fasally.Entities.Enums;

namespace Fasally.Contracts.Sellers;

public record SellerDashboardResponse(
    int TotalProducts,
    int ActiveProducts,
    int OutOfStockProducts,
    IEnumerable<SellerDashboardProductResponse> LatestProducts
);

public record SellerDashboardProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    int Stock,
    ProductStatus Status,
    DateTime CreatedAt
);
