using Fasally.Abstractions;
using Fasally.Contracts.Measurements;

namespace Fasally.Services;

public interface IMeasurementService
{
    Task<Result<MeasurementDto?>> GetMyMeasurementsAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<MeasurementDto>> SaveMeasurementsAsync(string userId, MeasurementDto request, CancellationToken cancellationToken = default);
}
