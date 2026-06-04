using Fasally.Entities.Enums;

namespace Fasally.Entities;

public class Product : AuditableEntity
{
    public string SellerProfileId { get; set; } = default!;
    public SellerProfile SellerProfile { get; set; } = default!;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductVariant> Variants { get; set; } = [];
    public ICollection<InventoryLog> InventoryLogs { get; set; } = [];
    public ICollection<ProposalProduct> ProposalProducts { get; set; } = [];
}
