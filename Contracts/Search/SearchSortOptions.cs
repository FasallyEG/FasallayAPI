namespace Fasally.Contracts.Search;

public static class SearchSortOptions
{
    public const string Newest = "newest";
    public const string PriceAscending = "price_asc";
    public const string PriceDescending = "price_desc";
    public const string NameAscending = "name_asc";
    public const string NameDescending = "name_desc";
    public const string RatingDescending = "rating_desc";
    public const string ExperienceDescending = "experience_desc";

    public static readonly IReadOnlyCollection<string> ProductSortOptions =
    [
        Newest,
        PriceAscending,
        PriceDescending,
        NameAscending,
        NameDescending
    ];

    public static readonly IReadOnlyCollection<string> TailorSortOptions =
    [
        RatingDescending,
        ExperienceDescending,
        NameAscending,
        NameDescending
    ];
}
