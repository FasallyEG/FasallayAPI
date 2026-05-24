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

    [HttpPost("{productId:guid}/images")]
    [Authorize]
    [HasPermission(Permissions.AddProductImage)]
    public async Task<IActionResult> AddImage(
        [FromRoute] Guid productId,
        [FromBody] AddProductImageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.AddProductImageAsync(userId, productId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize]
    [HasPermission(Permissions.DeleteProductImage)]
    public async Task<IActionResult> DeleteImage(
        [FromRoute] Guid productId,
        [FromRoute] Guid imageId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.DeleteProductImageAsync(userId, productId, imageId, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPost("{productId:guid}/variants")]
    [Authorize]
    [HasPermission(Permissions.AddProductVariant)]
    public async Task<IActionResult> AddVariant(
        [FromRoute] Guid productId,
        [FromBody] ProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.AddProductVariantAsync(userId, productId, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpPut("{productId:guid}/variants/{variantId:guid}")]
    [Authorize]
    [HasPermission(Permissions.UpdateProductVariant)]
    public async Task<IActionResult> UpdateVariant(
        [FromRoute] Guid productId,
        [FromRoute] Guid variantId,
        [FromBody] ProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.UpdateProductVariantAsync(userId, productId, variantId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpDelete("{productId:guid}/variants/{variantId:guid}")]
    [Authorize]
    [HasPermission(Permissions.DeleteProductVariant)]
    public async Task<IActionResult> DeleteVariant(
        [FromRoute] Guid productId,
        [FromRoute] Guid variantId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.DeleteProductVariantAsync(userId, productId, variantId, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpPut("{productId:guid}/stock")]
    [Authorize]
    [HasPermission(Permissions.UpdateProductStock)]
    public async Task<IActionResult> UpdateStock(
        [FromRoute] Guid productId,
        [FromBody] UpdateProductStockRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.UpdateProductStockAsync(userId, productId, request, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

    [HttpGet("{productId:guid}/inventory")]
    [Authorize]
    [HasPermission(Permissions.ViewProductInventory)]
    public async Task<IActionResult> GetInventory(
        [FromRoute] Guid productId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId()!;
        var result = await _productService.GetProductInventoryAsync(userId, productId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
