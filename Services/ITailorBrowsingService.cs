using Fasally.Abstractions;
using Fasally.Contracts.Tailors;

namespace Fasally.Services;

public interface ITailorBrowsingService
{
    // Returns approved tailors only
    Task<Result<PaginatedList<TailorListItemResponse>>> GetTailorsAsync(TailorFilterRequest request, CancellationToken cancellationToken = default);

    // Returns null if tailor is not approved
    Task<Result<TailorDetailsResponse>> GetTailorDetailsAsync(string tailorId, CancellationToken cancellationToken = default);
}
