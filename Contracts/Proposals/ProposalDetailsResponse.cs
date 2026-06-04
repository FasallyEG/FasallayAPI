using Fasally.Entities.Enums;

namespace Fasally.Contracts.Proposals;

public class ProposalDetailsResponse
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = default!;
    public string ClientName { get; set; } = string.Empty;
    public string TailorId { get; set; } = default!;
    public string TailorName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? ResponseDeadline { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? ProductionDeadline { get; set; }
    public ProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<ProposalImageResponse> Images { get; set; } = [];
    public IEnumerable<ProposalProductResponse> Products { get; set; } = [];
}
