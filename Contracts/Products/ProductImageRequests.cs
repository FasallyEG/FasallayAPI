using FluentValidation;

namespace Fasally.Contracts.Products;

public record AddProductImageRequest(
    string ImageUrl,
    string? AltText,
    int SortOrder = 0
);

public class AddProductImageRequestValidator : AbstractValidator<AddProductImageRequest>
{
    public AddProductImageRequestValidator()
    {
        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.AltText)
            .MaximumLength(200)
            .When(x => x.AltText is not null);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
