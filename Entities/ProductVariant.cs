namespace Fasally.Entities;

public class ProductVariant : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
