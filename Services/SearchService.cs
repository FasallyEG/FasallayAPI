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

public class SearchService(
    ApplicationDbContext context,
    ICategoryService categoryService) : ISearchService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICategoryService _categoryService = categoryService;

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

    public async Task<Result<PaginatedList<TailorListItemResponse>>> SearchTailorsAsync(
        TailorSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Normalize(request);

        var query = _context.Tailors
            .AsNoTracking()
            .Where(t => t.Status == ProfileStatus.Approved);

        if (!string.IsNullOrWhiteSpace(request.Q))
            query = query.Where(t =>
                t.User.FirstName.Contains(request.Q) ||
                t.User.LastName.Contains(request.Q) ||
                (t.User.FirstName + " " + t.User.LastName).Contains(request.Q) ||
                (t.Bio != null && t.Bio.Contains(request.Q)));

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.Categories.Any(c => c.Id == request.CategoryId.Value));

        if (request.Rating.HasValue)
            query = query.Where(t => t.AverageRating >= request.Rating.Value);

        query = ApplyTailorSort(query, request.Sort);

        var response = await PaginatedList<TailorListItemResponse>.CreateAsync(
            query.Select(TailorProjection),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(response);
    }

    public async Task<Result<SearchFiltersResponse>> GetFiltersAsync(
        CancellationToken cancellationToken = default)
    {
        var categoriesResult = await _categoryService.GetAllAsync(cancellationToken);
        var categories = categoriesResult.Value
            .OrderBy(c => c.Name)
            .ToList();

        var productPriceRange = await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active)
            .GroupBy(_ => 1)
            .Select(g => new PriceRangeResponse(
                g.Min(p => p.Price),
                g.Max(p => p.Price)))
            .FirstOrDefaultAsync(cancellationToken);

        var response = new SearchFiltersResponse(
            Products: new ProductFiltersMetadataResponse(
                Filters: ProductFilters,
                SortOptions: ProductSortOptions,
                PriceRange: productPriceRange,
                Categories: categories),
            Tailors: new TailorFiltersMetadataResponse(
                Filters: TailorFilters,
                SortOptions: TailorSortOptions),
            Pagination: new PaginationMetadataResponse(
                DefaultPageNumber: SearchValidationRules.DefaultPageNumber,
                DefaultPageSize: SearchValidationRules.DefaultPageSize,
                MaxPageSize: SearchValidationRules.MaxPageSize),
            Query: new QueryMetadataResponse(
                SearchValidationRules.MinQueryLength,
                SearchValidationRules.MaxQueryLength),
            Suggestions: new SuggestionsMetadataResponse(
                DefaultLimit: SearchValidationRules.DefaultSuggestionLimit,
                MaxLimit: SearchValidationRules.MaxSuggestionLimit));

        return Result.Success(response);
    }

    public async Task<Result<SearchSuggestionsResponse>> GetSuggestionsAsync(
        SearchSuggestionsRequest request,
        CancellationToken cancellationToken = default)
    {
        Normalize(request);

        var query = request.Q!;
        var limit = request.Limit;

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active && p.Name.Contains(query))
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                p.Id,
                p.Name
            })
            .Take(limit)
            .ToListAsync(cancellationToken);

        var categories = await _context.TailorCategories
            .AsNoTracking()
            .Where(c => c.Name.Contains(query))
            .OrderBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .Take(limit)
            .ToListAsync(cancellationToken);

        var tailors = await _context.Tailors
            .AsNoTracking()
            .Where(t =>
                t.Status == ProfileStatus.Approved &&
                (t.User.FirstName.Contains(query) ||
                 t.User.LastName.Contains(query) ||
                 (t.User.FirstName + " " + t.User.LastName).Contains(query)))
            .OrderBy(t => t.User.FirstName)
            .ThenBy(t => t.User.LastName)
            .Select(t => new
            {
                t.ApplicationUserId,
                t.User.FirstName,
                t.User.LastName
            })
            .Take(limit)
            .ToListAsync(cancellationToken);

        var suggestions = products
            .Select(p => new SearchSuggestionResponse(p.Name, "product", p.Id))
            .Concat(categories.Select(c => new SearchSuggestionResponse(c.Name, "category", c.Id)))
            .Concat(tailors.Select(t => new SearchSuggestionResponse(
                $"{t.FirstName} {t.LastName}".Trim(),
                "tailor",
                t.ApplicationUserId)))
            .Where(s => !string.IsNullOrWhiteSpace(s.Text))
            .GroupBy(s => new
            {
                Text = s.Text.Trim().ToUpperInvariant(),
                s.Type
            })
            .Select(g => g.First())
            .Take(limit)
            .ToList();

        return Result.Success(new SearchSuggestionsResponse(suggestions));
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
        request.Sort = NormalizeText(request.Sort) ?? SearchSortOptions.RatingDescending;
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

    private static IQueryable<Tailor> ApplyTailorSort(
        IQueryable<Tailor> query,
        string? sort) =>
        sort switch
        {
            SearchSortOptions.ExperienceDescending => query
                .OrderByDescending(t => t.ExperienceYears)
                .ThenByDescending(t => t.AverageRating)
                .ThenBy(t => t.User.FirstName)
                .ThenBy(t => t.User.LastName),

            SearchSortOptions.NameAscending => query
                .OrderBy(t => t.User.FirstName)
                .ThenBy(t => t.User.LastName),

            SearchSortOptions.NameDescending => query
                .OrderByDescending(t => t.User.FirstName)
                .ThenByDescending(t => t.User.LastName),

            _ => query
                .OrderByDescending(t => t.AverageRating)
                .ThenByDescending(t => t.TotalReviews)
                .ThenBy(t => t.User.FirstName)
                .ThenBy(t => t.User.LastName)
        };

    private static readonly Expression<Func<Tailor, TailorListItemResponse>> TailorProjection =
        tailor => new TailorListItemResponse(
            tailor.ApplicationUserId,
            tailor.User.FirstName + " " + tailor.User.LastName,
            tailor.User.ProfileImageUrl,
            tailor.Categories.Select(c => c.Name),
            tailor.ExperienceYears,
            tailor.AverageRating,
            tailor.TotalReviews);

    private static readonly FilterMetadataResponse[] ProductFilters =
    [
        new("categoryId", "select", true, "Category"),
        new("sellerId", "text", true, "Seller"),
        new("minPrice", "number", true, "Minimum price"),
        new("maxPrice", "number", true, "Maximum price"),
        new("inStock", "boolean", true, "In stock")
    ];

    private static readonly FilterMetadataResponse[] TailorFilters =
    [
        new("rating", "number", true, "Minimum rating"),
        new("categoryId", "select", true, "Category"),
        new("availability", "boolean", false, "Availability", "Not supported by the current data model"),
        new("location", "text", false, "Location", "Not supported by the current data model")
    ];

    private static readonly SortOptionResponse[] ProductSortOptions =
    [
        new(SearchSortOptions.Newest, "Newest"),
        new(SearchSortOptions.PriceAscending, "Price: Low to High"),
        new(SearchSortOptions.PriceDescending, "Price: High to Low"),
        new(SearchSortOptions.NameAscending, "Name: A to Z"),
        new(SearchSortOptions.NameDescending, "Name: Z to A")
    ];

    private static readonly SortOptionResponse[] TailorSortOptions =
    [
        new(SearchSortOptions.RatingDescending, "Rating: High to Low"),
        new(SearchSortOptions.ExperienceDescending, "Experience: High to Low"),
        new(SearchSortOptions.NameAscending, "Name: A to Z"),
        new(SearchSortOptions.NameDescending, "Name: Z to A")
    ];
}
