using Fasally.Abstractions;
using Fasally.Contracts.Proposals;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class TailorProposalService(
    ApplicationDbContext context,
    ILogger<TailorProposalService> logger) : ITailorProposalService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<TailorProposalService> _logger = logger;

    public async Task<Result<IEnumerable<ProposalListItemResponse>>> GetTailorProposalsAsync(
        string tailorId,
        CancellationToken cancellationToken = default)
    {
        var tailorExists = await _context.Tailors
            .AnyAsync(t => t.ApplicationUserId == tailorId, cancellationToken);

        if (!tailorExists)
            return Result.Failure<IEnumerable<ProposalListItemResponse>>(TailorErrors.TailorNotFound);

        var proposals = await BaseProposalQuery()
            .Where(p => p.TailorId == tailorId)
            .OrderByDescending(p => p.CreatedAt)
            .ProjectToType<ProposalListItemResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProposalListItemResponse>>(proposals);
    }

    public async Task<Result> AcceptProposalAsync(
        string tailorId,
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        var proposal = await GetTailorProposalForManagementAsync(tailorId, proposalId, cancellationToken);
        if (proposal.IsFailure)
            return proposal;

        if (proposal.Value.Status != ProposalStatus.Pending)
            return Result.Failure(ProposalErrors.InvalidStatusTransition);

        proposal.Value.AcceptedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Proposal {ProposalId} accepted by tailor {TailorId}", proposalId, tailorId);

        return Result.Success();
    }

    public async Task<Result> RejectProposalAsync(
        string tailorId,
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        var proposalResult = await GetTailorProposalForManagementAsync(tailorId, proposalId, cancellationToken);
        if (proposalResult.IsFailure)
            return proposalResult;

        var proposal = proposalResult.Value;
        if (proposal.Status != ProposalStatus.Pending || proposal.AcceptedAt is not null)
            return Result.Failure(ProposalErrors.InvalidStatusTransition);

        proposal.Status = ProposalStatus.Rejected;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Proposal {ProposalId} rejected by tailor {TailorId}", proposalId, tailorId);

        return Result.Success();
    }

    public async Task<Result> FinalizeProposalAsync(
        string tailorId,
        Guid proposalId,
        FinalizeProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var proposalResult = await GetTailorProposalForManagementAsync(tailorId, proposalId, cancellationToken);
        if (proposalResult.IsFailure)
            return proposalResult;

        var proposal = proposalResult.Value;
        if (proposal.Status != ProposalStatus.Pending || proposal.AcceptedAt is null)
            return Result.Failure(ProposalErrors.InvalidStatusTransition);

        var duplicateProductExists = request.Products
            .GroupBy(p => p.ProductId)
            .Any(g => g.Count() > 1);

        if (duplicateProductExists)
            return Result.Failure(ProposalErrors.DuplicateProduct);

        var productIds = request.Products
            .Select(p => p.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && !p.IsDeleted && p.Status == ProductStatus.Active)
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        if (products.Count != productIds.Count)
            return Result.Failure(ProposalErrors.ProductNotFound);

        proposal.TotalPrice = request.TotalPrice;
        proposal.ProductionDeadline = request.ProductionDeadline;
        proposal.Status = ProposalStatus.AwaitingClientApproval;

        _context.ProposalProducts.RemoveRange(proposal.Products);
        proposal.Products = request.Products
            .Select(p => new ProposalProduct
            {
                ProposalId = proposalId,
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                UnitPriceAtProposal = products[p.ProductId].Price
            })
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Proposal {ProposalId} finalized by tailor {TailorId}", proposalId, tailorId);

        return Result.Success();
    }

    private IQueryable<Proposal> BaseProposalQuery() =>
        _context.Proposals
            .AsNoTracking()
            .Include(p => p.Client)
            .Include(p => p.Tailor)
                .ThenInclude(t => t.User)
            .Include(p => p.Images)
            .Include(p => p.Products)
                .ThenInclude(p => p.Product);

    private async Task<Result<Proposal>> GetTailorProposalForManagementAsync(
        string tailorId,
        Guid proposalId,
        CancellationToken cancellationToken)
    {
        var proposal = await _context.Proposals
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

        if (proposal is null)
            return Result.Failure<Proposal>(ProposalErrors.ProposalNotFound);

        if (proposal.TailorId != tailorId)
            return Result.Failure<Proposal>(ProposalErrors.NotProposalTailor);

        return Result.Success(proposal);
    }
}
