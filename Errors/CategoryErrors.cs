using Fasally.Abstractions;

namespace Fasally.Errors;

public record CategoryErrors
{
    public static readonly Error CategoryNotFound =
        new("Category.CategoryNotFound", "Category is not found", StatusCodes.Status404NotFound);

    public static readonly Error DuplicatedCategory =
        new("Category.DuplicatedCategory", "Another category with the same name already exists", StatusCodes.Status409Conflict);
}
