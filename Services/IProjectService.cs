using Fasally.Abstractions;
using Fasally.Contracts.Projects;

namespace Fasally.Services;

public interface IProjectService
{
    Task<Result<IEnumerable<ProjectListItemResponse>>> GetMyProjectsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<ProjectDetailsResponse>> GetProjectDetailsAsync(string userId, Guid projectId, CancellationToken cancellationToken = default);
    Task<Result> StartProjectAsync(string tailorId, Guid projectId, CancellationToken cancellationToken = default);
    Task<Result> CompleteProjectAsync(string tailorId, Guid projectId, CancellationToken cancellationToken = default);
}
