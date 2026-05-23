using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Authentication.Filters;
using Fasally.Contracts.Products;
using Fasally.Extensions;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(IProductService productService) : ControllerBase
{
    private readonly IProductService _productService = productService;

    [HttpGet("")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] ProductFilterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("{productId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _productService.GetProductAsync(productId, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPost("")]
    [Authorize]
    [HasPermission(Permissions.CreateProduct)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.CreateProductAsync(userId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { productId = result.Value.Id }, result.Value)
            : result.ToProblem();
    }

    [HttpPut("{productId:guid}")]
    [Authorize]
    [HasPermission(Permissions.UpdateProduct)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.UpdateProductAsync(userId, productId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{productId:guid}")]
    [Authorize]
    [HasPermission(Permissions.DeleteProduct)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.DeleteProductAsync(userId, productId, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }
}
