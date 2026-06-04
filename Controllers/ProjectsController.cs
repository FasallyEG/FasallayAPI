using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    private readonly IProjectService _projectService = projectService;

    [HttpGet("")]
    [HasPermission(Permissions.ViewMyProjects)]
    public async Task<IActionResult> GetMyProjects(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _projectService.GetMyProjectsAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("{projectId:guid}")]
    [HasPermission(Permissions.ViewMyProjects)]
    public async Task<IActionResult> GetProjectDetails(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _projectService.GetProjectDetailsAsync(userId, projectId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("{projectId:guid}/start")]
    [HasPermission(Permissions.StartProject)]
    public async Task<IActionResult> StartProject(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _projectService.StartProjectAsync(userId, projectId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{projectId:guid}/complete")]
    [HasPermission(Permissions.CompleteProject)]
    public async Task<IActionResult> CompleteProject(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _projectService.CompleteProjectAsync(userId, projectId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
