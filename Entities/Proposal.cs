using Fasally.Entities.Enums;

namespace Fasally.Entities;

public class Proposal
{
    public Guid Id { get; set; }

    public string ClientId { get; set; } = default!;
    public ApplicationUser Client { get; set; } = default!;

    public string TailorId { get; set; } = default!;
    public Tailor Tailor { get; set; } = default!;

    public string Description { get; set; } = string.Empty;
    public DateTime? ResponseDeadline { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? ProductionDeadline { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProposalImage> Images { get; set; } = [];
    public ICollection<ProposalProduct> Products { get; set; } = [];
    public Project? Project { get; set; }
}
