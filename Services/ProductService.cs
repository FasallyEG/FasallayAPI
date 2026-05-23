using Fasally.Abstractions;
using Fasally.Contracts.Products;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Fasally.Services;

public class ProductService(
    ApplicationDbContext context,
    ILogger<ProductService> logger) : IProductService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<ProductService> _logger = logger;

    public async Task<Result<PaginatedList<ProductResponse>>> GetProductsAsync(
        ProductFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(
            BaseProductQuery().Where(p => p.Status == ProductStatus.Active),
            request);

        var products = await PaginatedList<ProductResponse>.CreateAsync(
            query.Select(ProductProjection),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(products);
    }

    public async Task<Result<ProductResponse>> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await BaseProductQuery()
            .FirstOrDefaultAsync(p => p.Id == productId && p.Status == ProductStatus.Active, cancellationToken);

        if (product is null)
            return Result.Failure<ProductResponse>(ProductErrors.ProductNotFound);

        return Result.Success(ToResponse(product));
    }

    public async Task<Result<ProductResponse>> CreateProductAsync(
        string userId,
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var sellerExists = await _context.SellerProfiles
            .AnyAsync(s => s.ApplicationUserId == userId, cancellationToken);

        if (!sellerExists)
            return Result.Failure<ProductResponse>(SellerErrors.SellerNotFound);

        var validation = await ValidateProductInputAsync(
            request.Price,
            request.Stock,
            request.CategoryId,
            cancellationToken);

        if (validation.IsFailure)
            return Result.Failure<ProductResponse>(validation.Error);

        var product = new Product
        {
            SellerProfileId = userId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            Status = request.Status
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} created by seller {SellerId}", product.Id, userId);

        var created = await BaseProductQuery()
            .FirstAsync(p => p.Id == product.Id, cancellationToken);

        return Result.Success(ToResponse(created));
    }

    public async Task<Result> UpdateProductAsync(
        string userId,
        Guid productId,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);

        if (product is null)
            return Result.Failure(ProductErrors.ProductNotFound);

        if (product.SellerProfileId != userId)
            return Result.Failure(ProductErrors.NotProductOwner);

        var validation = await ValidateProductInputAsync(
            request.Price,
            request.Stock,
            request.CategoryId,
            cancellationToken);

        if (validation.IsFailure)
            return validation;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.CategoryId = request.CategoryId;
        product.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} updated by seller {SellerId}", productId, userId);

        return Result.Success();
    }

    public async Task<Result> DeleteProductAsync(
        string userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);

        if (product is null)
            return Result.Failure(ProductErrors.ProductNotFound);

        if (product.SellerProfileId != userId)
            return Result.Failure(ProductErrors.NotProductOwner);

        product.IsDeleted = true;
        product.Status = ProductStatus.Inactive;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} deleted by seller {SellerId}", productId, userId);

        return Result.Success();
    }

    public async Task<Result<PaginatedList<ProductResponse>>> GetSellerProductsAsync(
        string sellerId,
        ProductFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var sellerExists = await _context.SellerProfiles
            .AnyAsync(s => s.ApplicationUserId == sellerId, cancellationToken);

        if (!sellerExists)
            return Result.Failure<PaginatedList<ProductResponse>>(SellerErrors.SellerNotFound);

        request.SellerId = sellerId;

        var query = ApplyFilters(
            BaseProductQuery().Where(p => p.Status == ProductStatus.Active),
            request);

        var products = await PaginatedList<ProductResponse>.CreateAsync(
            query.Select(ProductProjection),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(products);
    }

    public async Task<Result<PaginatedList<ProductResponse>>> GetCurrentSellerProductsAsync(
        string userId,
        ProductFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var sellerExists = await _context.SellerProfiles
            .AnyAsync(s => s.ApplicationUserId == userId, cancellationToken);

        if (!sellerExists)
            return Result.Failure<PaginatedList<ProductResponse>>(SellerErrors.SellerNotFound);

        request.SellerId = userId;

        var query = ApplyFilters(BaseProductQuery(), request);

        var products = await PaginatedList<ProductResponse>.CreateAsync(
            query.Select(ProductProjection),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(products);
    }

    private async Task<Result> ValidateProductInputAsync(
        decimal price,
        int stock,
        int? categoryId,
        CancellationToken cancellationToken)
    {
        if (price <= 0)
            return Result.Failure(ProductErrors.InvalidPrice);

        if (stock < 0)
            return Result.Failure(ProductErrors.InvalidStock);

        if (categoryId.HasValue)
        {
            var categoryExists = await _context.TailorCategories
                .AnyAsync(c => c.Id == categoryId.Value, cancellationToken);

            if (!categoryExists)
                return Result.Failure(CategoryErrors.CategoryNotFound);
        }

        return Result.Success();
    }

    private IQueryable<Product> BaseProductQuery() =>
        _context.Products
            .AsNoTracking()
            .Include(p => p.SellerProfile)
            .Include(p => p.Category)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

    private static IQueryable<Product> ApplyFilters(
        IQueryable<Product> query,
        ProductFilterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Name.Contains(request.Search) ||
                (p.Description != null && p.Description.Contains(request.Search)));

        if (!string.IsNullOrWhiteSpace(request.SellerId))
            query = query.Where(p => p.SellerProfileId == request.SellerId);

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.InStock.HasValue)
            query = request.InStock.Value
                ? query.Where(p => p.Stock > 0)
                : query.Where(p => p.Stock == 0);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        return query;
    }

    private static ProductResponse ToResponse(Product product) =>
        new(
            product.Id,
            product.SellerProfileId,
            product.SellerProfile.StoreName,
            product.CategoryId,
            product.Category?.Name,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.Status,
            product.CreatedAt,
            product.UpdatedAt);

    private static readonly Expression<Func<Product, ProductResponse>> ProductProjection =
        product => new ProductResponse(
            product.Id,
            product.SellerProfileId,
            product.SellerProfile.StoreName,
            product.CategoryId,
            product.Category == null ? null : product.Category.Name,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.Status,
            product.CreatedAt,
            product.UpdatedAt);
}
