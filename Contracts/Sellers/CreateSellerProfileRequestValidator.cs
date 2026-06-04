using FluentValidation;

namespace Fasally.Contracts.Sellers;

public class CreateSellerProfileRequestValidator : AbstractValidator<CreateSellerProfileRequest>
{
    public CreateSellerProfileRequestValidator()
    {
        RuleFor(x => x.StoreName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.BusinessPhone)
            .MaximumLength(50)
            .When(x => x.BusinessPhone is not null);

        RuleFor(x => x.BusinessEmail)
            .EmailAddress()
            .MaximumLength(256)
            .When(x => x.BusinessEmail is not null);

        RuleFor(x => x.ShopImageUrl)
            .MaximumLength(500)
            .When(x => x.ShopImageUrl is not null);
    }
}
