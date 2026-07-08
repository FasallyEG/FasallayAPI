using Fasally.Abstractions;
using Fasally.Contracts.Categories;
using Fasally.Contracts.Products;
using Fasally.Contracts.Search;
using Fasally.Contracts.Tailors;

namespace Fasally.Services;

public class SearchService : ISearchService
{
    public Task<Result<PaginatedList<ProductResponse>>> SearchProductsAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Normalize(request);

        var response = new PaginatedList<ProductResponse>(
            [],
            request.PageNumber,
            0,
            request.PageSize);

        return Task.FromResult(Result.Success(response));
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
}
