using FluentValidation;

namespace Fasally.Contracts.Measurements;

public class MeasurementDtoValidator : AbstractValidator<MeasurementDto>
{
    public MeasurementDtoValidator()
    {
        RuleFor(x => x.Chest)
            .GreaterThan(0)
            .When(x => x.Chest.HasValue);

        RuleFor(x => x.Waist)
            .GreaterThan(0)
            .When(x => x.Waist.HasValue);

        RuleFor(x => x.Hip)
            .GreaterThan(0)
            .When(x => x.Hip.HasValue);

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .When(x => x.Length.HasValue);

        RuleFor(x => x.Sleeve)
            .GreaterThan(0)
            .When(x => x.Sleeve.HasValue);

        RuleFor(x => x.AdditionalMeasurements)
            .MaximumLength(1000);
    }
}
