using FluentValidation;

namespace Fasally.Contracts.Search;

public class ProductSearchRequest
{
    public string? Q { get; set; }
    public int? CategoryId { get; set; }
    public string? SellerId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string? Sort { get; set; } = SearchSortOptions.Newest;
    public int PageNumber { get; set; } = SearchValidationRules.DefaultPageNumber;
    public int PageSize { get; set; } = SearchValidationRules.DefaultPageSize;
}

public class TailorSearchRequest
{
    public string? Q { get; set; }
    public double? Rating { get; set; }
    public int? CategoryId { get; set; }
    public string? Availability { get; set; }
    public string? Location { get; set; }
    public string? Sort { get; set; } = SearchSortOptions.RatingDescending;
    public int PageNumber { get; set; } = SearchValidationRules.DefaultPageNumber;
    public int PageSize { get; set; } = SearchValidationRules.DefaultPageSize;
}

public class SearchSuggestionsRequest
{
    public string? Q { get; set; }
    public int Limit { get; set; } = SearchValidationRules.DefaultSuggestionLimit;
}

public class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
{
    public ProductSearchRequestValidator()
    {
        RuleFor(x => x.Q)
            .Must(SearchRequestValidation.HasValidQueryLength)
            .WithMessage($"Q must be between {SearchValidationRules.MinQueryLength} and {SearchValidationRules.MaxQueryLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice must be less than or equal to MaxPrice");

        RuleFor(x => x.Sort)
            .Must(sort => string.IsNullOrWhiteSpace(sort) || SearchSortOptions.ProductSortOptions.Contains(sort))
            .WithMessage("Unsupported product sort option");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, SearchValidationRules.MaxPageSize);
    }
}

public class TailorSearchRequestValidator : AbstractValidator<TailorSearchRequest>
{
    public TailorSearchRequestValidator()
    {
        RuleFor(x => x.Q)
            .Must(SearchRequestValidation.HasValidQueryLength)
            .WithMessage($"Q must be between {SearchValidationRules.MinQueryLength} and {SearchValidationRules.MaxQueryLength} characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Q));

        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 5)
            .When(x => x.Rating.HasValue);

        RuleFor(x => x.Availability)
            .Must(string.IsNullOrWhiteSpace)
            .WithMessage("Availability filtering is not supported yet");

        RuleFor(x => x.Location)
            .Must(string.IsNullOrWhiteSpace)
            .WithMessage("Location filtering is not supported yet");

        RuleFor(x => x.Sort)
            .Must(sort => string.IsNullOrWhiteSpace(sort) || SearchSortOptions.TailorSortOptions.Contains(sort))
            .WithMessage("Unsupported tailor sort option");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, SearchValidationRules.MaxPageSize);
    }
}

public class SearchSuggestionsRequestValidator : AbstractValidator<SearchSuggestionsRequest>
{
    public SearchSuggestionsRequestValidator()
    {
        RuleFor(x => x.Q)
            .NotEmpty()
            .Must(SearchRequestValidation.HasValidQueryLength)
            .WithMessage($"Q must be between {SearchValidationRules.MinQueryLength} and {SearchValidationRules.MaxQueryLength} characters");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, SearchValidationRules.MaxSuggestionLimit);
    }
}

internal static class SearchRequestValidation
{
    public static bool HasValidQueryLength(string? value)
    {
        var length = value?.Trim().Length ?? 0;

        return length >= SearchValidationRules.MinQueryLength
            && length <= SearchValidationRules.MaxQueryLength;
    }
}
