using Fasally.Entities.Enums;

namespace Fasally.Contracts.Proposals;

public record ProposalListItemResponse(
    Guid Id,
    string ClientId,
    string ClientName,
    string TailorId,
    string TailorName,
    ProposalStatus Status,
    decimal? TotalPrice,
    DateTime? ResponseDeadline,
    DateTime? ProductionDeadline,
    DateTime CreatedAt
);
