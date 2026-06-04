using Fasally.Abstractions;
using Fasally.Contracts.Measurements;
using Fasally.Entities;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class MeasurementService(ApplicationDbContext context) : IMeasurementService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<MeasurementDto?>> GetMyMeasurementsAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var measurement = await _context.ClientMeasurements
            .AsNoTracking()
            .Where(m => m.ApplicationUserId == userId)
            .ProjectToType<MeasurementDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return Result.Success<MeasurementDto?>(measurement);
    }

    public async Task<Result<MeasurementDto>> SaveMeasurementsAsync(
        string userId,
        MeasurementDto request,
        CancellationToken cancellationToken = default)
    {
        var measurement = await _context.ClientMeasurements
            .FirstOrDefaultAsync(m => m.ApplicationUserId == userId, cancellationToken);

        if (measurement is null)
        {
            measurement = request.Adapt<ClientMeasurement>();
            measurement.ApplicationUserId = userId;
            _context.ClientMeasurements.Add(measurement);
        }
        else
        {
            request.Adapt(measurement);
        }

        measurement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(measurement.Adapt<MeasurementDto>());
    }
}
