using FluentValidation;

namespace Fasally.Contracts.Tailors;

public record UpdateTailorRequest(
    int ExperienceYears,
    List<int>? CategoryIds,
    string? Bio
);

public class UpdateTailorRequestValidator : AbstractValidator<UpdateTailorRequest>
{
    public UpdateTailorRequestValidator()
    {
        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(60);

        RuleFor(x => x.Bio)
            .MaximumLength(1000)
            .When(x => x.Bio is not null);

        RuleFor(x => x.CategoryIds)
            .Must(ids => ids is null || ids.Count > 0)
            .WithMessage("CategoryIds must not be an empty list");
    }
}
