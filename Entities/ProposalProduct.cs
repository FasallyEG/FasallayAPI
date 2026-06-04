namespace Fasally.Entities;

public class ProposalProduct
{
    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = default!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = default!;

    public int Quantity { get; set; }
    public decimal UnitPriceAtProposal { get; set; }
}
