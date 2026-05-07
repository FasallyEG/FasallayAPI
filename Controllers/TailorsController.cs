using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Tailors;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TailorsController(ITailorService tailorService) : ControllerBase
{
    private readonly ITailorService _tailorService = tailorService;

    // ── Tailor: own profile ───────────────────────────────────────────────

    [HttpPost("profile")]
    [HasPermission(Permissions.CreateTailorProfile)]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateTailorRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorService.CreateTailorProfileAsync(userId, request, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    [HttpPut("profile")]
    [HasPermission(Permissions.UpdateTailorProfile)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateTailorRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorService.UpdateTailorProfileAsync(userId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    // ── Tailor: portfolio ─────────────────────────────────────────────────

    [HttpGet("my-portfolio")]
    [HasPermission(Permissions.ViewMyPortfolio)]
    public async Task<IActionResult> GetMyPortfolio(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorService.GetMyPortfolioAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("my-portfolio")]
    [HasPermission(Permissions.AddPortfolioItem)]
    public async Task<IActionResult> AddPortfolioItem(
        [FromBody] CreatePortfolioItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _tailorService.AddPortfolioItemAsync(userId, request, cancellationToken);

        return result.IsSuccess ? Ok() : result.ToProblem();
    }

    // ── Admin: approve / reject ───────────────────────────────────────────

    [HttpPut("{tailorId}/approve")]
    [HasPermission(Permissions.ApproveTailor)]
    public async Task<IActionResult> Approve(string tailorId, CancellationToken cancellationToken)
    {
        var result = await _tailorService.ApproveTailorAsync(tailorId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{tailorId}/reject")]
    [HasPermission(Permissions.RejectTailor)]
    public async Task<IActionResult> Reject(string tailorId, CancellationToken cancellationToken)
    {
        var result = await _tailorService.RejectTailorAsync(tailorId, cancellationToken);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
