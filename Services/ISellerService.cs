using Fasally.Abstractions;
using Fasally.Contracts.Sellers;

namespace Fasally.Services;

public interface ISellerService
{
    Task<Result<SellerProfileResponse>> CreateSellerProfileAsync(string userId, CreateSellerProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<SellerProfileResponse>> GetCurrentSellerProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<PublicSellerProfileResponse>> GetSellerProfileAsync(string sellerId, CancellationToken cancellationToken = default);
    Task<Result> UpdateSellerProfileAsync(string userId, UpdateSellerProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<SellerDashboardResponse>> GetCurrentSellerDashboardAsync(string userId, CancellationToken cancellationToken = default);
}
