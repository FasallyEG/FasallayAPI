using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Tailors;
using Fasally.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TailorBrowsingController(ITailorBrowsingService browsingService) : ControllerBase
{
    private readonly ITailorBrowsingService _browsingService = browsingService;

    [HttpGet]
    [HasPermission(Permissions.ViewTailors)]
    public async Task<IActionResult> GetTailors(
        [FromQuery] TailorFilterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _browsingService.GetTailorsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{tailorId}")]
    [HasPermission(Permissions.ViewTailorDetails)]
    public async Task<IActionResult> GetTailorDetails(
        string tailorId,
        CancellationToken cancellationToken)
    {
        var result = await _browsingService.GetTailorDetailsAsync(tailorId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
