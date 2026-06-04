using FluentValidation;

namespace Fasally.Contracts.Addresses;

public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
{
    public CreateAddressRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Area)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Street)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.BuildingNumber)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Floor)
            .MaximumLength(50);

        RuleFor(x => x.ApartmentNumber)
            .MaximumLength(50);

        RuleFor(x => x.Notes)
            .MaximumLength(500);
    }
}
