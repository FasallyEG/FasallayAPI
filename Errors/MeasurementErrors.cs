using Fasally.Abstractions;

namespace Fasally.Errors;

public static class MeasurementErrors
{
    public static readonly Error MeasurementNotFound =
        new("Measurement.NotFound", "Measurements not found", StatusCodes.Status404NotFound);
}
