using Fasally.Entities.Enums;

namespace Fasally.Contracts.Products;

public record ProductResponse(
    Guid Id,
    string SellerId,
    string SellerStoreName,
    int? CategoryId,
    string? CategoryName,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    ProductStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
