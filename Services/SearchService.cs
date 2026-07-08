using Fasally.Abstractions;
using Fasally.Contracts.Categories;
using Fasally.Contracts.Products;
using Fasally.Contracts.Search;
using Fasally.Contracts.Tailors;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Fasally.Services;

public class SearchService(ApplicationDbContext context) : ISearchService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<PaginatedList<ProductResponse>>> SearchProductsAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Normalize(request);

        var query = _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.Q))
            query = query.Where(p =>
                p.Name.Contains(request.Q) ||
                (p.Description != null && p.Description.Contains(request.Q)));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.SellerId))
            query = query.Where(p => p.SellerProfileId == request.SellerId);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.InStock.HasValue)
            query = request.InStock.Value
                ? query.Where(p => p.Stock > 0)
                : query.Where(p => p.Stock == 0);

        query = ApplyProductSort(query, request.Sort);

        var response = await PaginatedList<ProductResponse>.CreateAsync(
            query.Select(ProductProjection),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(response);
    }

    public Task<Result<PaginatedList<TailorListItemResponse>>> SearchTailorsAsync(
        TailorSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Normalize(request);

        var response = new PaginatedList<TailorListItemResponse>(
            [],
            request.PageNumber,
            0,
            request.PageSize);

        return Task.FromResult(Result.Success(response));
    }

    public Task<Result<SearchFiltersResponse>> GetFiltersAsync(
        CancellationToken cancellationToken = default)
    {
        var response = new SearchFiltersResponse(
            ProductCategories: Array.Empty<CategoryResponse>(),
            ProductPriceRange: null,
            ProductSortOptions: SearchSortOptions.ProductSortOptions,
            TailorSortOptions: SearchSortOptions.TailorSortOptions,
            SupportedTailorFilters: ["rating", "categoryId"],
            FutureTailorFilters: ["availability", "location"]);

        return Task.FromResult(Result.Success(response));
    }

    public Task<Result<IEnumerable<SearchSuggestionResponse>>> GetSuggestionsAsync(
        SearchSuggestionsRequest request,
        CancellationToken cancellationToken = default)
    {
        Normalize(request);

        return Task.FromResult(Result.Success<IEnumerable<SearchSuggestionResponse>>([]));
    }

    private static void Normalize(ProductSearchRequest request)
    {
        request.Q = NormalizeText(request.Q);
        request.SellerId = NormalizeText(request.SellerId);
        request.Sort = NormalizeText(request.Sort) ?? SearchSortOptions.Newest;
    }

    private static void Normalize(TailorSearchRequest request)
    {
        request.Q = NormalizeText(request.Q);
        request.Availability = NormalizeText(request.Availability);
        request.Location = NormalizeText(request.Location);
        request.Sort = NormalizeText(request.Sort) ?? SearchSortOptions.Newest;
    }

    private static void Normalize(SearchSuggestionsRequest request)
    {
        request.Q = NormalizeText(request.Q);
    }

    private static string? NormalizeText(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static IQueryable<Product> ApplyProductSort(
        IQueryable<Product> query,
        string? sort) =>
        sort switch
        {
            SearchSortOptions.PriceAscending => query.OrderBy(p => p.Price).ThenByDescending(p => p.CreatedAt),
            SearchSortOptions.PriceDescending => query.OrderByDescending(p => p.Price).ThenByDescending(p => p.CreatedAt),
            SearchSortOptions.NameAscending => query.OrderBy(p => p.Name).ThenByDescending(p => p.CreatedAt),
            SearchSortOptions.NameDescending => query.OrderByDescending(p => p.Name).ThenByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

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
}
