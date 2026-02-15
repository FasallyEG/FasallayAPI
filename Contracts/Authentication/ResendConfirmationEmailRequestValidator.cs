using FluentValidation;

namespace Fasally.Contracts.Authentication;
public class ResendConfirmationEmailRequestValidator:AbstractValidator<ResendConfirmationEmailRequest>
    {
    public ResendConfirmationEmailRequestValidator()

        {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        }
    }
