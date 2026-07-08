using Fasally.Abstractions;
using Fasally.Contracts.Products;
using Fasally.Contracts.Search;
using Fasally.Contracts.Tailors;

namespace Fasally.Services;

public interface ISearchService
{
    Task<Result<PaginatedList<ProductResponse>>> SearchProductsAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PaginatedList<TailorListItemResponse>>> SearchTailorsAsync(
        TailorSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<SearchFiltersResponse>> GetFiltersAsync(
        CancellationToken cancellationToken = default);

    Task<Result<IEnumerable<SearchSuggestionResponse>>> GetSuggestionsAsync(
        SearchSuggestionsRequest request,
        CancellationToken cancellationToken = default);
}
