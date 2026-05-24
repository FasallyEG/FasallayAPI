using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Users;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("me")]
[ApiController]
[Authorize]
public class AccountController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("")]
    public async Task<IActionResult> Info()
    {
        var result = await _userService.GetProfileAsync(User.GetUserId()!);
        return Ok(result.Value);
    }

    [HttpPut("info")]
    public async Task<IActionResult> Info([FromBody] UpdateProfileRequest request)
    {
        await _userService.UpdateProfileAsync(User.GetUserId()!, request);
        return NoContent();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> Change([FromBody] ChangePasswordRequest request)
    {
        var result = await _userService.ChangePasswordAsync(User.GetUserId()!, request);
        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("request-upgrade")]
    [HasPermission(Permissions.CompleteProfile)]
    public async Task<IActionResult> RequestUpgrade([FromBody] RequestUpgradeRequest request)
    {
        var userId = User.GetUserId()!;
        var result = await _userService.RequestUpgradeAsync(userId, request);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}
