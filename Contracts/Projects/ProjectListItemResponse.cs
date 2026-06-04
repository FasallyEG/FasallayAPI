using Fasally.Entities.Enums;

namespace Fasally.Contracts.Projects;

public record ProjectListItemResponse(
    Guid Id,
    Guid ProposalId,
    string ClientId,
    string ClientName,
    string TailorId,
    string TailorName,
    ProjectStatus Status,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt
);
