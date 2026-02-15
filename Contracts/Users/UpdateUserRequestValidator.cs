using FluentValidation;

namespace Fasally.Contracts.Users;

public class UpdateUserRequestValidator:AbstractValidator<UpdateUserRequest>
    {
    public UpdateUserRequestValidator()
        {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(3,100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(3,100);

        }
    }