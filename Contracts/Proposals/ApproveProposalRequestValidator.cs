using FluentValidation;

namespace Fasally.Contracts.Proposals;

public class ApproveProposalRequestValidator : AbstractValidator<ApproveProposalRequest>
{
    public ApproveProposalRequestValidator()
    {
        RuleFor(x => x.Measurement)
            .NotNull()
            .SetValidator(new Measurements.MeasurementDtoValidator());
    }
}
