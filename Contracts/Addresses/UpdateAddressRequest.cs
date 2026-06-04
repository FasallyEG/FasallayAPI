namespace Fasally.Contracts.Addresses;

public record UpdateAddressRequest(
    string Name,
    string City,
    string Area,
    string Street,
    string BuildingNumber,
    string? Floor,
    string? ApartmentNumber,
    string? Notes
);
