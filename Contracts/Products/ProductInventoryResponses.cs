namespace Fasally.Contracts.Products;

public record ProductInventoryResponse(
    Guid ProductId,
    int CurrentStock,
    IEnumerable<InventoryLogResponse> Logs
);

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
