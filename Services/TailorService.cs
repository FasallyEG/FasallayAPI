using Fasally.Abstractions;
using Fasally.Abstractions.Consts;
using Fasally.Contracts.Tailors;
using Fasally.Entities;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class TailorService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager) : ITailorService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<Result> CreateTailorProfileAsync(
        string userId,
        CreateTailorRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        // Must be approved (have Tailor role) before submitting profile
        var isTailor = await _userManager.IsInRoleAsync(user, DefaultRoles.Tailor);
        if (!isTailor)
            return Result.Failure(TailorErrors.NotApprovedYet);

        var exists = await _context.Tailors
            .AnyAsync(t => t.ApplicationUserId == userId, cancellationToken);

        if (exists)
            return Result.Failure(TailorErrors.TailorAlreadyExists);

        List<Category> categories = new();
        if (request.CategoryIds is { Count: > 0 })
        {
            categories = await _context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            if (categories.Count != request.CategoryIds.Count)
                return Result.Failure(TailorErrors.InvalidCategories);
        }

        var tailor = request.Adapt<Tailor>();
        tailor.ApplicationUserId = userId;
        tailor.Status            = ProfileStatus.Approved; // already approved at this point
        tailor.Categories        = categories;

        _context.Tailors.Add(tailor);

        user.IsProfileCompleted = true;

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateTailorProfileAsync(
        string userId,
        UpdateTailorRequest request,
        CancellationToken cancellationToken = default)
    {
        var tailor = await _context.Tailors
            .Include(t => t.Categories)
            .FirstOrDefaultAsync(t => t.ApplicationUserId == userId, cancellationToken);

        if (tailor is null)
            return Result.Failure(TailorErrors.TailorNotFound);

        if (request.CategoryIds is { Count: > 0 })
        {
            var categories = await _context.Categories
                .Where(c => request.CategoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);

            if (categories.Count != request.CategoryIds.Count)
                return Result.Failure(TailorErrors.InvalidCategories);

            tailor.Categories = categories;
        }
        else if (request.CategoryIds is not null)
        {
            tailor.Categories = new List<Category>();
        }

        request.Adapt(tailor);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> AddPortfolioItemAsync(
        string userId,
        CreatePortfolioItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var tailorExists = await _context.Tailors
            .AnyAsync(t => t.ApplicationUserId == userId, cancellationToken);

        if (!tailorExists)
            return Result.Failure(TailorErrors.TailorNotFound);

        var item = request.Adapt<PortfolioItem>();
        item.TailorId = userId;

        _context.PortfolioItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IEnumerable<PortfolioItemResponse>>> GetMyPortfolioAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var tailorExists = await _context.Tailors
            .AnyAsync(t => t.ApplicationUserId == userId, cancellationToken);

        if (!tailorExists)
            return Result.Failure<IEnumerable<PortfolioItemResponse>>(TailorErrors.TailorNotFound);

        var items = await _context.PortfolioItems
            .AsNoTracking()
            .Where(p => p.TailorId == userId)
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<PortfolioItemResponse>>(items.Adapt<List<PortfolioItemResponse>>());
    }

    // Admin: approve upgrade request → assign Tailor role
    public async Task<Result> ApproveTailorAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        if (user.PendingProfileType is null)
            return Result.Failure(UserErrors.NoUpgradeRequested);

        var alreadyTailor = await _userManager.IsInRoleAsync(user, DefaultRoles.Tailor);
        if (alreadyTailor)
            return Result.Failure(TailorErrors.ProfileAlreadyApproved);

        // Assign role — user can now submit their tailor profile
        await _userManager.AddToRoleAsync(user, DefaultRoles.Tailor);

        user.PendingProfileType = null;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    // Admin: reject upgrade request → clear pending type
    public async Task<Result> RejectTailorAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        if (user.PendingProfileType is null)
            return Result.Failure(UserErrors.NoUpgradeRequested);

        user.PendingProfileType = null;
        user.IsProfileCompleted = true;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}
