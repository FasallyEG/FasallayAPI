using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Sellers;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SellersController(ISellerService sellerService) : ControllerBase
{
    private readonly ISellerService _sellerService = sellerService;

    [HttpPost("profile")]
    [HasPermission(Permissions.CreateSellerProfile)]
    public async Task<IActionResult> CreateProfile(
        [FromBody] CreateSellerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _sellerService.CreateSellerProfileAsync(userId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { sellerId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpGet("me")]
    [HasPermission(Permissions.ViewMySellerProfile)]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _sellerService.GetCurrentSellerProfileAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{sellerId}")]
    [HasPermission(Permissions.ViewSellerProfile)]
    public async Task<IActionResult> GetById(
        string sellerId,
        CancellationToken cancellationToken)
    {
        var result = await _sellerService.GetSellerProfileAsync(sellerId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("profile")]
    [HasPermission(Permissions.UpdateSellerProfile)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateSellerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _sellerService.UpdateSellerProfileAsync(userId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
