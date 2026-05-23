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

    public async Task<Result<ProductImageResponse>> AddProductImageAsync(
        string userId,
        Guid productId,
        AddProductImageRequest request,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return Result.Failure<ProductImageResponse>(productResult.Error);

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = request.ImageUrl,
            AltText = request.AltText,
            SortOrder = request.SortOrder
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Image {ImageId} added to product {ProductId} by seller {SellerId}", image.Id, productId, userId);

        return Result.Success(ToImageResponse(image));
    }

    public async Task<Result> DeleteProductImageAsync(
        string userId,
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return productResult;

        var image = await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == productId && !i.IsDeleted, cancellationToken);

        if (image is null)
            return Result.Failure(ProductErrors.ProductImageNotFound);

        image.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Image {ImageId} deleted from product {ProductId} by seller {SellerId}", imageId, productId, userId);

        return Result.Success();
    }

    public async Task<Result<ProductVariantResponse>> AddProductVariantAsync(
        string userId,
        Guid productId,
        ProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return Result.Failure<ProductVariantResponse>(productResult.Error);

        var variant = new ProductVariant
        {
            ProductId = productId,
            Type = request.Type,
            Value = request.Value
        };

        _context.ProductVariants.Add(variant);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variant {VariantId} added to product {ProductId} by seller {SellerId}", variant.Id, productId, userId);

        return Result.Success(ToVariantResponse(variant));
    }

    public async Task<Result> UpdateProductVariantAsync(
        string userId,
        Guid productId,
        Guid variantId,
        ProductVariantRequest request,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return productResult;

        var variant = await _context.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId && !v.IsDeleted, cancellationToken);

        if (variant is null)
            return Result.Failure(ProductErrors.ProductVariantNotFound);

        variant.Type = request.Type;
        variant.Value = request.Value;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variant {VariantId} updated on product {ProductId} by seller {SellerId}", variantId, productId, userId);

        return Result.Success();
    }

    public async Task<Result> DeleteProductVariantAsync(
        string userId,
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return productResult;

        var variant = await _context.ProductVariants
            .FirstOrDefaultAsync(v => v.Id == variantId && v.ProductId == productId && !v.IsDeleted, cancellationToken);

        if (variant is null)
            return Result.Failure(ProductErrors.ProductVariantNotFound);

        variant.IsDeleted = true;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variant {VariantId} deleted from product {ProductId} by seller {SellerId}", variantId, productId, userId);

        return Result.Success();
    }

    public async Task<Result> UpdateProductStockAsync(
        string userId,
        Guid productId,
        UpdateProductStockRequest request,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return productResult;

        var product = productResult.Value;
        var oldStock = product.Stock;

        product.Stock = request.Stock;

        _context.InventoryLogs.Add(new InventoryLog
        {
            ProductId = productId,
            OldStock = oldStock,
            NewStock = request.Stock,
            ChangeAmount = request.Stock - oldStock,
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Stock updated for product {ProductId} by seller {SellerId}: {OldStock} -> {NewStock}",
            productId,
            userId,
            oldStock,
            request.Stock);

        return Result.Success();
    }

    public async Task<Result<ProductInventoryResponse>> GetProductInventoryAsync(
        string userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var productResult = await GetOwnedProductForManagementAsync(userId, productId, cancellationToken);
        if (productResult.IsFailure)
            return Result.Failure<ProductInventoryResponse>(productResult.Error);

        var logs = await _context.InventoryLogs
            .AsNoTracking()
            .Where(l => l.ProductId == productId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new InventoryLogResponse(
                l.Id,
                l.ProductId,
                l.OldStock,
                l.NewStock,
                l.ChangeAmount,
                l.Reason,
                l.CreatedAt,
                l.CreatedById))
            .ToListAsync(cancellationToken);

        return Result.Success(new ProductInventoryResponse(
            productId,
            productResult.Value.Stock,
            logs));
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

    private async Task<Result<Product>> GetOwnedProductForManagementAsync(
        string userId,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);

        if (product is null)
            return Result.Failure<Product>(ProductErrors.ProductNotFound);

        if (product.SellerProfileId != userId)
            return Result.Failure<Product>(ProductErrors.NotProductOwner);

        return Result.Success(product);
    }

    private IQueryable<Product> BaseProductQuery() =>
        _context.Products
            .AsNoTracking()
            .Include(p => p.SellerProfile)
            .Include(p => p.Category)
            .Include(p => p.Images.Where(i => !i.IsDeleted))
            .Include(p => p.Variants.Where(v => !v.IsDeleted))
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
            product.UpdatedAt,
            product.Images
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.SortOrder)
                .Select(ToImageResponse),
            product.Variants
                .Where(v => !v.IsDeleted)
                .Select(ToVariantResponse));

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
            product.UpdatedAt,
            product.Images
                .Where(i => !i.IsDeleted)
                .OrderBy(i => i.SortOrder)
                .Select(i => new ProductImageResponse(
                    i.Id,
                    i.ImageUrl,
                    i.AltText,
                    i.SortOrder)),
            product.Variants
                .Where(v => !v.IsDeleted)
                .Select(v => new ProductVariantResponse(
                    v.Id,
                    v.Type,
                    v.Value)));

    private static ProductImageResponse ToImageResponse(ProductImage image) =>
        new(
            image.Id,
            image.ImageUrl,
            image.AltText,
            image.SortOrder);

    private static ProductVariantResponse ToVariantResponse(ProductVariant variant) =>
        new(
            variant.Id,
            variant.Type,
            variant.Value);
}
