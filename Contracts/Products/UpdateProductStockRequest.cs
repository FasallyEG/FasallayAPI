namespace Fasally.Contracts.Products;

public record UpdateProductStockRequest(
    int Stock,
    string? Reason
);
