using Fasally.Abstractions;
using Fasally.Contracts.Proposals;

namespace Fasally.Services;

public interface IProposalService
{
    Task<Result<Guid>> CreateProposalAsync(string clientId, CreateProposalRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ProposalListItemResponse>>> GetMyProposalsAsync(string clientId, CancellationToken cancellationToken = default);
    Task<Result<ProposalDetailsResponse>> GetProposalDetailsAsync(string clientId, Guid proposalId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> ApproveProposalAsync(string clientId, Guid proposalId, ApproveProposalRequest request, CancellationToken cancellationToken = default);
    Task<Result> RejectProposalAsync(string clientId, Guid proposalId, CancellationToken cancellationToken = default);
}
