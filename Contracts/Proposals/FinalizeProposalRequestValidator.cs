using FluentValidation;

namespace Fasally.Contracts.Proposals;

public class FinalizeProposalRequestValidator : AbstractValidator<FinalizeProposalRequest>
{
    public FinalizeProposalRequestValidator()
    {
        RuleFor(x => x.TotalPrice)
            .GreaterThan(0);

        RuleFor(x => x.ProductionDeadline)
            .NotEmpty();

        RuleFor(x => x.Products)
            .NotEmpty();

        RuleForEach(x => x.Products)
            .SetValidator(new ProposalProductRequestValidator());
    }
}
