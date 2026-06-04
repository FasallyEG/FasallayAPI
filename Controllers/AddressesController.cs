using Fasally.Abstractions;
using Fasally.Contracts.Addresses;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AddressesController(IAddressService addressService) : ControllerBase
{
    private readonly IAddressService _addressService = addressService;

    [HttpGet("")]
    public async Task<IActionResult> GetMyAddresses(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _addressService.GetMyAddressesAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{addressId:guid}")]
    public async Task<IActionResult> GetAddress(
        [FromRoute] Guid addressId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _addressService.GetAddressAsync(userId, addressId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    public async Task<IActionResult> AddAddress(
        [FromBody] CreateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _addressService.AddAddressAsync(userId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAddress), new { addressId = result.Value }, result.Value)
            : result.ToProblem();
    }

    [HttpPut("{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(
        [FromRoute] Guid addressId,
        [FromBody] UpdateAddressRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _addressService.UpdateAddressAsync(userId, addressId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(
        [FromRoute] Guid addressId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _addressService.DeleteAddressAsync(userId, addressId, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
