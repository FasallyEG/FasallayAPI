namespace Fasally.Contracts.Products;

public record InventoryLogResponse(
    Guid Id,
    Guid ProductId,
    int OldStock,
    int NewStock,
    int ChangeAmount,
    string? Reason,
    DateTime CreatedAt,
    string? CreatedById
);
