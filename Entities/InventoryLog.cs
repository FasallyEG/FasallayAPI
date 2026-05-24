namespace Fasally.Entities;

public class InventoryLog : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int OldStock { get; set; }
    public int NewStock { get; set; }
    public int ChangeAmount { get; set; }
    public string? Reason { get; set; }
}
