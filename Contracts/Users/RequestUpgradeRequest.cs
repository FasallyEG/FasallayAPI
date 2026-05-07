using Fasally.Entities.Enums;
using FluentValidation;

namespace Fasally.Contracts.Users;

public record RequestUpgradeRequest(
    ProfileType ProfileType
);

public class RequestUpgradeRequestValidator : AbstractValidator<RequestUpgradeRequest>
{
    public RequestUpgradeRequestValidator()
    {
        RuleFor(x => x.ProfileType)
            .IsInEnum()
            .WithMessage("Invalid profile type");
    }
}
