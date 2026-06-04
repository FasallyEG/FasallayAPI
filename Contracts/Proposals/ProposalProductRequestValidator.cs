using FluentValidation;

namespace Fasally.Contracts.Proposals;

public class ProposalProductRequestValidator : AbstractValidator<ProposalProductRequest>
{
    public ProposalProductRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
