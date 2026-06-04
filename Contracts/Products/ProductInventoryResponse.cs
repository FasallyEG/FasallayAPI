namespace Fasally.Contracts.Products;

public record ProductInventoryResponse(
    Guid ProductId,
    int CurrentStock,
    IEnumerable<InventoryLogResponse> Logs
);
