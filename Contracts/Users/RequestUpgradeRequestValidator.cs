using FluentValidation;

namespace Fasally.Contracts.Users;

public class RequestUpgradeRequestValidator : AbstractValidator<RequestUpgradeRequest>
{
    public RequestUpgradeRequestValidator()
    {
        RuleFor(x => x.ProfileType)
            .IsInEnum()
            .WithMessage("Invalid profile type");
    }
}
