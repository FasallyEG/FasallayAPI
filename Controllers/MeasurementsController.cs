using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Measurements;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MeasurementsController(IMeasurementService measurementService) : ControllerBase
{
    private readonly IMeasurementService _measurementService = measurementService;

    [HttpGet("me")]
    [HasPermission(Permissions.ViewMyMeasurements)]
    public async Task<IActionResult> GetMyMeasurements(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _measurementService.GetMyMeasurementsAsync(userId, cancellationToken);
        return Ok(result.Value);
    }

    [HttpPut("me")]
    [HasPermission(Permissions.UpdateMyMeasurements)]
    public async Task<IActionResult> SaveMeasurements(
        [FromBody] MeasurementDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _measurementService.SaveMeasurementsAsync(userId, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
