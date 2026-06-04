namespace Fasally.Contracts.Proposals;

public record ProposalProductResponse(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
