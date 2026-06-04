namespace Fasally.Contracts.Proposals;

public class FinalizeProposalRequest
{
    public decimal TotalPrice { get; set; }
    public DateTime ProductionDeadline { get; set; }
    public List<ProposalProductRequest> Products { get; set; } = [];
}
