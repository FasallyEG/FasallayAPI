using Fasally.Abstractions;
using Fasally.Contracts.Addresses;
using Fasally.Entities;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class AddressService(
    ApplicationDbContext context,
    ILogger<AddressService> logger) : IAddressService
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<AddressService> _logger = logger;

    public async Task<Result<IEnumerable<AddressResponse>>> GetMyAddressesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var addresses = await _context.Addresses
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Name)
            .ProjectToType<AddressResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<AddressResponse>>(addresses);
    }

    public async Task<Result<AddressResponse>> GetAddressAsync(
        string userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var address = await _context.Addresses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address is null)
            return Result.Failure<AddressResponse>(AddressErrors.AddressNotFound);

        return Result.Success(address.Adapt<AddressResponse>());
    }

    public async Task<Result<Guid>> AddAddressAsync(
        string userId,
        CreateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var address = request.Adapt<Address>();
        address.UserId = userId;

        _context.Addresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} created for user {UserId}", address.Id, userId);

        return Result.Success(address.Id);
    }

    public async Task<Result> UpdateAddressAsync(
        string userId,
        Guid addressId,
        UpdateAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address is null)
            return Result.Failure(AddressErrors.AddressNotFound);

        request.Adapt(address);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} updated for user {UserId}", addressId, userId);

        return Result.Success();
    }

    public async Task<Result> DeleteAddressAsync(
        string userId,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        var address = await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId, cancellationToken);

        if (address is null)
            return Result.Failure(AddressErrors.AddressNotFound);

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Address {AddressId} deleted for user {UserId}", addressId, userId);

        return Result.Success();
    }
}
