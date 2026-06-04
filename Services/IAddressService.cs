using Fasally.Abstractions;
using Fasally.Contracts.Addresses;

namespace Fasally.Services;

public interface IAddressService
{
    Task<Result<IEnumerable<AddressResponse>>> GetMyAddressesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AddressResponse>> GetAddressAsync(string userId, Guid addressId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> AddAddressAsync(string userId, CreateAddressRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateAddressAsync(string userId, Guid addressId, UpdateAddressRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAddressAsync(string userId, Guid addressId, CancellationToken cancellationToken = default);
}
