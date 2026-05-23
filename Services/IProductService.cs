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
}
