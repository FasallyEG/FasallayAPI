using Fasally.Entities.Enums;

namespace Fasally.Entities;

public class Project
{
    public Guid Id { get; set; }

    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = default!;

    public ProjectStatus Status { get; set; } = ProjectStatus.Approved;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ProjectMeasurement MeasurementSnapshot { get; set; } = default!;
}
