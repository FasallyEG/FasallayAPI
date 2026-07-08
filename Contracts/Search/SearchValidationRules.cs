namespace Fasally.Contracts.Search;

public static class SearchValidationRules
{
    public const int MinQueryLength = 2;
    public const int MaxQueryLength = 100;
    public const int MaxPageSize = 100;
    public const int MaxSuggestionLimit = 20;
    public const int MaxFilterTextLength = 100;
}
