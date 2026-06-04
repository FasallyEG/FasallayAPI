namespace Fasally.Contracts.Sellers;

public record SellerDashboardResponse(
    int TotalProducts,
    int ActiveProducts,
    int OutOfStockProducts,
    IEnumerable<SellerDashboardProductResponse> LatestProducts
);
