using Fasally.Contracts.Measurements;
using Fasally.Contracts.Proposals;
using Fasally.Entities.Enums;

namespace Fasally.Contracts.Projects;

public class ProjectDetailsResponse
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string ClientId { get; set; } = default!;
    public string ClientName { get; set; } = string.Empty;
    public string TailorId { get; set; } = default!;
    public string TailorName { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ProposalDescription { get; set; } = string.Empty;
    public decimal? TotalPrice { get; set; }
    public DateTime? ProductionDeadline { get; set; }
    public MeasurementDto MeasurementSnapshot { get; set; } = default!;
    public IEnumerable<ProposalProductResponse> Products { get; set; } = [];
}
