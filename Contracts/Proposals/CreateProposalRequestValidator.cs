using FluentValidation;

namespace Fasally.Contracts.Proposals;

public class CreateProposalRequestValidator : AbstractValidator<CreateProposalRequest>
{
    public CreateProposalRequestValidator()
    {
        RuleFor(x => x.TailorId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(2000);

        RuleForEach(x => x.ImageUrls)
            .NotEmpty()
            .MaximumLength(500);
    }
}
