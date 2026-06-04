using FluentValidation;

namespace Fasally.Contracts.Tailors;

public class CreatePortfolioItemRequestValidator : AbstractValidator<CreatePortfolioItemRequest>
{
    public CreatePortfolioItemRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.ImageUrls)
            .NotEmpty()
            .WithMessage("At least one image URL is required");
    }
}
