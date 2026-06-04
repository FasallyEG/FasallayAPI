using Fasally.Abstractions;
using Fasally.Contracts.Proposals;

namespace Fasally.Services;

public interface ITailorProposalService
{
    Task<Result<IEnumerable<ProposalListItemResponse>>> GetTailorProposalsAsync(string tailorId, CancellationToken cancellationToken = default);
    Task<Result> AcceptProposalAsync(string tailorId, Guid proposalId, CancellationToken cancellationToken = default);
    Task<Result> RejectProposalAsync(string tailorId, Guid proposalId, CancellationToken cancellationToken = default);
    Task<Result> FinalizeProposalAsync(string tailorId, Guid proposalId, FinalizeProposalRequest request, CancellationToken cancellationToken = default);
}
