using Fasally.Contracts.Categories;

namespace Fasally.Contracts.Search;

public record SearchFiltersResponse(
    ProductFiltersMetadataResponse Products,
    TailorFiltersMetadataResponse Tailors,
    PaginationMetadataResponse Pagination,
    QueryMetadataResponse Query,
    SuggestionsMetadataResponse Suggestions
);

public record ProductFiltersMetadataResponse(
    IEnumerable<FilterMetadataResponse> Filters,
    IEnumerable<SortOptionResponse> SortOptions,
    PriceRangeResponse? PriceRange,
    IEnumerable<CategoryResponse> Categories
);

public record TailorFiltersMetadataResponse(
    IEnumerable<FilterMetadataResponse> Filters,
    IEnumerable<SortOptionResponse> SortOptions
);

public record FilterMetadataResponse(
    string Key,
    string Type,
    bool Supported,
    string Label,
    string? Reason = null
);

public record SortOptionResponse(string Value, string Label);

public record PriceRangeResponse(decimal Min, decimal Max);

public record PaginationMetadataResponse(
    int DefaultPageNumber,
    int DefaultPageSize,
    int MaxPageSize
);

public record QueryMetadataResponse(int MinLength, int MaxLength);

public record SuggestionsMetadataResponse(int DefaultLimit, int MaxLimit);

public record SearchSuggestionsResponse(IEnumerable<SearchSuggestionResponse> Suggestions);

public record SearchSuggestionResponse(
    string Text,
    string Type,
    object? Id
);
