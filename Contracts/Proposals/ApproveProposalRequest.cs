using Fasally.Contracts.Measurements;

namespace Fasally.Contracts.Proposals;

public class ApproveProposalRequest
{
    public MeasurementDto Measurement { get; set; } = default!;
    public bool UpdateProfileMeasurement { get; set; }
}
