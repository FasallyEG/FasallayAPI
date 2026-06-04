using Fasally.Abstractions;
using Fasally.Contracts.Proposals;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class ProposalService(
    ApplicationDbContext context,
    ILogger<ProposalService> logger) : IProposalService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<ProposalService> _logger = logger;

    public async Task<Result<Guid>> CreateProposalAsync(
        string clientId,
        CreateProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var tailorExists = await _context.Tailors
            .AnyAsync(t => t.ApplicationUserId == request.TailorId && t.Status == ProfileStatus.Approved, cancellationToken);

        if (!tailorExists)
            return Result.Failure<Guid>(TailorErrors.TailorNotFound);

        var proposal = request.Adapt<Proposal>();
        proposal.ClientId = clientId;

        _context.Proposals.Add(proposal);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Proposal {ProposalId} created by client {ClientId} for tailor {TailorId}", proposal.Id, clientId, request.TailorId);

        return Result.Success(proposal.Id);
    }

    public async Task<Result<IEnumerable<ProposalListItemResponse>>> GetMyProposalsAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var proposals = await BaseProposalQuery()
            .Where(p => p.ClientId == clientId)
            .OrderByDescending(p => p.CreatedAt)
            .ProjectToType<ProposalListItemResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProposalListItemResponse>>(proposals);
    }

    public async Task<Result<ProposalDetailsResponse>> GetProposalDetailsAsync(
        string clientId,
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        var proposal = await BaseProposalQuery()
            .Where(p => p.Id == proposalId && p.ClientId == clientId)
            .ProjectToType<ProposalDetailsResponse>()
            .FirstOrDefaultAsync(cancellationToken);

        if (proposal is null)
            return Result.Failure<ProposalDetailsResponse>(ProposalErrors.ProposalNotFound);

        return Result.Success(proposal);
    }

    public async Task<Result<Guid>> ApproveProposalAsync(
        string clientId,
        Guid proposalId,
        ApproveProposalRequest request,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _context.Proposals
            .Include(p => p.Project)
            .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

        if (proposal is null)
            return Result.Failure<Guid>(ProposalErrors.ProposalNotFound);

        if (proposal.ClientId != clientId)
            return Result.Failure<Guid>(ProposalErrors.NotProposalClient);

        if (proposal.Status != ProposalStatus.AwaitingClientApproval || proposal.Project is not null)
            return Result.Failure<Guid>(ProposalErrors.InvalidStatusTransition);

        if (request.UpdateProfileMeasurement)
            await SaveClientMeasurementAsync(clientId, request, cancellationToken);

        var project = new Project
        {
            ProposalId = proposal.Id,
            Status = ProjectStatus.Approved,
            MeasurementSnapshot = request.Measurement.Adapt<ProjectMeasurement>()
        };

        proposal.Status = ProposalStatus.Approved;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Proposal {ProposalId} approved by client {ClientId}; project {ProjectId} created", proposalId, clientId, project.Id);

        return Result.Success(project.Id);
    }

    public async Task<Result> RejectProposalAsync(
        string clientId,
        Guid proposalId,
        CancellationToken cancellationToken = default)
    {
        var proposal = await _context.Proposals
            .FirstOrDefaultAsync(p => p.Id == proposalId, cancellationToken);

        if (proposal is null)
            return Result.Failure(ProposalErrors.ProposalNotFound);

        if (proposal.ClientId != clientId)
            return Result.Failure(ProposalErrors.NotProposalClient);

        if (proposal.Status is ProposalStatus.Approved or ProposalStatus.Rejected or ProposalStatus.Expired)
            return Result.Failure(ProposalErrors.InvalidStatusTransition);

        proposal.Status = ProposalStatus.Rejected;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Proposal {ProposalId} rejected by client {ClientId}", proposalId, clientId);

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

    private async Task SaveClientMeasurementAsync(
        string clientId,
        ApproveProposalRequest request,
        CancellationToken cancellationToken)
    {
        var measurement = await _context.ClientMeasurements
            .FirstOrDefaultAsync(m => m.ApplicationUserId == clientId, cancellationToken);

        if (measurement is null)
        {
            measurement = request.Measurement.Adapt<ClientMeasurement>();
            measurement.ApplicationUserId = clientId;
            _context.ClientMeasurements.Add(measurement);
        }
        else
        {
            request.Measurement.Adapt(measurement);
        }

        measurement.UpdatedAt = DateTime.UtcNow;
    }
}
