using Fasally.Abstractions;

namespace Fasally.Errors;

public static class AddressErrors
{
    public static readonly Error AddressNotFound =
        new("Address.NotFound", "Address not found", StatusCodes.Status404NotFound);

    public static readonly Error AddressInUse =
        new("Address.InUse", "Address is used by an order and cannot be deleted", StatusCodes.Status409Conflict);
}
