using FluentValidation;

namespace Fasally.Contracts.Users;

public class ForgetPasswordRequestValidator:AbstractValidator<ForgetPasswordRequest>
    {
    public ForgetPasswordRequestValidator()
        {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        }
    }
