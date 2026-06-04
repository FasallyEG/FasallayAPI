using Fasally.Entities.Enums;

namespace Fasally.Contracts.Products;

public class ProductFilterRequest
{
    public string? Search { get; set; }
    public string? SellerId { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public ProductStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
