using Fasally.Contracts.Categories;

namespace Fasally.Contracts.Search;

public record SearchFiltersResponse(
    IEnumerable<CategoryResponse> ProductCategories,
    PriceRangeResponse? ProductPriceRange,
    IEnumerable<string> ProductSortOptions,
    IEnumerable<string> TailorSortOptions,
    IEnumerable<string> SupportedTailorFilters,
    IEnumerable<string> FutureTailorFilters
);

public record PriceRangeResponse(decimal MinPrice, decimal MaxPrice);

public record SearchSuggestionResponse(
    string Type,
    string Text,
    string? Value
);
