using Fasally.Abstractions;
using Fasally.Contracts.Products;

namespace Fasally.Services;

public interface IProductService
{
    Task<Result<PaginatedList<ProductResponse>>> GetProductsAsync(ProductFilterRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> GetProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<Result<ProductResponse>> CreateProductAsync(string userId, CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateProductAsync(string userId, Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductAsync(string userId, Guid productId, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<ProductResponse>>> GetSellerProductsAsync(string sellerId, ProductFilterRequest request, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<ProductResponse>>> GetCurrentSellerProductsAsync(string userId, ProductFilterRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductImageResponse>> AddProductImageAsync(string userId, Guid productId, AddProductImageRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductImageAsync(string userId, Guid productId, Guid imageId, CancellationToken cancellationToken = default);
    Task<Result<ProductVariantResponse>> AddProductVariantAsync(string userId, Guid productId, ProductVariantRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateProductVariantAsync(string userId, Guid productId, Guid variantId, ProductVariantRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteProductVariantAsync(string userId, Guid productId, Guid variantId, CancellationToken cancellationToken = default);
}
