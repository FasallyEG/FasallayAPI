using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Contracts.Sellers;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class SellerService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<SellerService> logger) : ISellerService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<SellerService> _logger = logger;

    public async Task<Result<SellerProfileResponse>> CreateSellerProfileAsync(
        string userId,
        CreateSellerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure<SellerProfileResponse>(UserErrors.UserNotFound);

        var isSeller = await _userManager.IsInRoleAsync(user, DefaultRoles.Seller);
        if (!isSeller)
            return Result.Failure<SellerProfileResponse>(SellerErrors.NotApprovedYet);

        var exists = await _context.SellerProfiles
            .AnyAsync(s => s.ApplicationUserId == userId, cancellationToken);

        if (exists)
            return Result.Failure<SellerProfileResponse>(SellerErrors.SellerAlreadyExists);

        var seller = new SellerProfile
        {
            ApplicationUserId = userId,
            StoreName = request.StoreName,
            Description = request.Description,
            BusinessPhone = request.BusinessPhone,
            BusinessEmail = request.BusinessEmail,
            ShopImageUrl = request.ShopImageUrl,
            Status = ProfileStatus.Approved
        };

        _context.SellerProfiles.Add(seller);
        user.IsProfileCompleted = true;

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seller profile created for user {UserId}", userId);

        return Result.Success(ToResponse(seller));
    }

    public async Task<Result<SellerProfileResponse>> GetCurrentSellerProfileAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId, cancellationToken);

        if (seller is null)
            return Result.Failure<SellerProfileResponse>(SellerErrors.SellerNotFound);

        return Result.Success(ToResponse(seller));
    }

    public async Task<Result<PublicSellerProfileResponse>> GetSellerProfileAsync(
        string sellerId,
        CancellationToken cancellationToken = default)
    {
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(s => s.ApplicationUserId == sellerId, cancellationToken);

        if (seller is null)
            return Result.Failure<PublicSellerProfileResponse>(SellerErrors.SellerNotFound);

        return Result.Success(ToPublicResponse(seller));
    }

    public async Task<Result> UpdateSellerProfileAsync(
        string userId,
        UpdateSellerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var seller = await _context.SellerProfiles
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId, cancellationToken);

        if (seller is null)
            return Result.Failure(SellerErrors.SellerNotFound);

        seller.StoreName = request.StoreName;
        seller.Description = request.Description;
        seller.BusinessPhone = request.BusinessPhone;
        seller.BusinessEmail = request.BusinessEmail;
        seller.ShopImageUrl = request.ShopImageUrl;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seller profile updated for user {UserId}", userId);

        return Result.Success();
    }

    public async Task<Result<SellerDashboardResponse>> GetCurrentSellerDashboardAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var sellerExists = await _context.SellerProfiles
            .AnyAsync(s => s.ApplicationUserId == userId, cancellationToken);

        if (!sellerExists)
            return Result.Failure<SellerDashboardResponse>(SellerErrors.SellerNotFound);

        var products = _context.Products
            .AsNoTracking()
            .Where(p => p.SellerProfileId == userId && !p.IsDeleted);

        var totalProducts = await products.CountAsync(cancellationToken);
        var activeProducts = await products
            .CountAsync(p => p.Status == ProductStatus.Active, cancellationToken);
        var outOfStockProducts = await products
            .CountAsync(p => p.Stock == 0, cancellationToken);

        var latestProducts = await products
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new SellerDashboardProductResponse(
                p.Id,
                p.Name,
                p.Price,
                p.Stock,
                p.Status,
                p.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result.Success(new SellerDashboardResponse(
            totalProducts,
            activeProducts,
            outOfStockProducts,
            latestProducts));
    }

    private static SellerProfileResponse ToResponse(SellerProfile seller) =>
        new(
            seller.ApplicationUserId,
            seller.StoreName,
            seller.Description,
            seller.BusinessPhone,
            seller.BusinessEmail,
            seller.ShopImageUrl,
            seller.Status,
            seller.IsVerified,
            seller.AverageRating,
            seller.TotalReviews);

    private static PublicSellerProfileResponse ToPublicResponse(SellerProfile seller) =>
        new(
            seller.ApplicationUserId,
            seller.StoreName,
            seller.Description,
            seller.ShopImageUrl,
            seller.IsVerified,
            seller.AverageRating,
            seller.TotalReviews);
}
