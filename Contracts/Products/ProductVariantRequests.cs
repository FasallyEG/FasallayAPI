using FluentValidation;

namespace Fasally.Contracts.Products;

public record ProductVariantRequest(
    string Type,
    string Value
);

public class ProductVariantRequestValidator : AbstractValidator<ProductVariantRequest>
{
    public ProductVariantRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Value)
            .NotEmpty()
            .MaximumLength(200);
    }
}
