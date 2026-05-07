using Fasally.Abstractions;
using Fasally.Contracts.Tailors;

namespace Fasally.Services;

public interface ITailorService
{
    Task<Result>                                     CreateTailorProfileAsync(string userId, CreateTailorRequest request, CancellationToken cancellationToken = default);
    Task<Result>                                     UpdateTailorProfileAsync(string userId, UpdateTailorRequest request, CancellationToken cancellationToken = default);
    Task<Result>                                     AddPortfolioItemAsync(string userId, CreatePortfolioItemRequest request, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PortfolioItemResponse>>> GetMyPortfolioAsync(string userId, CancellationToken cancellationToken = default);

    // Admin
    Task<Result> ApproveTailorAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> RejectTailorAsync(string userId, CancellationToken cancellationToken = default);
}
