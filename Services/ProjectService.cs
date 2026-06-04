using Fasally.Abstractions;
using Fasally.Contracts.Projects;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class ProjectService(
    ApplicationDbContext context,
    ILogger<ProjectService> logger) : IProjectService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<ProjectService> _logger = logger;

    public async Task<Result<IEnumerable<ProjectListItemResponse>>> GetMyProjectsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var projects = await BaseProjectQuery()
            .Where(p => p.Proposal.ClientId == userId || p.Proposal.TailorId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ProjectToType<ProjectListItemResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<ProjectListItemResponse>>(projects);
    }

    public async Task<Result<ProjectDetailsResponse>> GetProjectDetailsAsync(
        string userId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var project = await BaseProjectQuery()
            .Where(p => p.Id == projectId && (p.Proposal.ClientId == userId || p.Proposal.TailorId == userId))
            .ProjectToType<ProjectDetailsResponse>()
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
            return Result.Failure<ProjectDetailsResponse>(ProjectErrors.ProjectNotFound);

        return Result.Success(project);
    }

    public async Task<Result> StartProjectAsync(
        string tailorId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var projectResult = await GetTailorProjectForManagementAsync(tailorId, projectId, cancellationToken);
        if (projectResult.IsFailure)
            return projectResult;

        var project = projectResult.Value;
        if (project.Status != ProjectStatus.Approved)
            return Result.Failure(ProjectErrors.InvalidStatusTransition);

        project.Status = ProjectStatus.InProgress;
        project.StartedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} started by tailor {TailorId}", projectId, tailorId);

        return Result.Success();
    }

    public async Task<Result> CompleteProjectAsync(
        string tailorId,
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var projectResult = await GetTailorProjectForManagementAsync(tailorId, projectId, cancellationToken);
        if (projectResult.IsFailure)
            return projectResult;

        var project = projectResult.Value;
        if (project.Status != ProjectStatus.InProgress)
            return Result.Failure(ProjectErrors.InvalidStatusTransition);

        project.Status = ProjectStatus.Completed;
        project.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Project {ProjectId} completed by tailor {TailorId}", projectId, tailorId);

        return Result.Success();
    }

    private IQueryable<Project> BaseProjectQuery() =>
        _context.Projects
            .AsNoTracking()
            .Include(p => p.Proposal)
                .ThenInclude(p => p.Client)
            .Include(p => p.Proposal)
                .ThenInclude(p => p.Tailor)
                    .ThenInclude(t => t.User)
            .Include(p => p.Proposal)
                .ThenInclude(p => p.Products)
                    .ThenInclude(p => p.Product)
            .Include(p => p.MeasurementSnapshot);

    private async Task<Result<Project>> GetTailorProjectForManagementAsync(
        string tailorId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Proposal)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project is null)
            return Result.Failure<Project>(ProjectErrors.ProjectNotFound);

        if (project.Proposal.TailorId != tailorId)
            return Result.Failure<Project>(ProjectErrors.NotProjectTailor);

        return Result.Success(project);
    }
}
