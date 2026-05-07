using Fasally.Abstractions;
using Fasally.Contracts.Tailors;
using Fasally.Entities.Enums;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class TailorBrowsingService(ApplicationDbContext context) : ITailorBrowsingService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<PaginatedList<TailorListItemResponse>>> GetTailorsAsync(
        TailorFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tailors
            .Where(t => t.Status == ProfileStatus.Approved)
            .Include(t => t.User)
            .Include(t => t.Categories)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(t =>
                t.User.FirstName.Contains(request.Search) ||
                t.User.LastName.Contains(request.Search) ||
                (t.Bio != null && t.Bio.Contains(request.Search)));

        if (request.CategoryId.HasValue)
            query = query.Where(t => t.Categories.Any(c => c.Id == request.CategoryId.Value));

        if (request.MinRating.HasValue)
            query = query.Where(t => t.AverageRating >= request.MinRating.Value);

        if (request.MinExperienceYears.HasValue)
            query = query.Where(t => t.ExperienceYears >= request.MinExperienceYears.Value);

        var result = await PaginatedList<TailorListItemResponse>.CreateAsync(
            query.ProjectToType<TailorListItemResponse>(),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(result);
    }

    public async Task<Result<TailorDetailsResponse>> GetTailorDetailsAsync(
        string tailorId,
        CancellationToken cancellationToken = default)
    {
        var tailor = await _context.Tailors
            .Where(t => t.ApplicationUserId == tailorId && t.Status == ProfileStatus.Approved)
            .Include(t => t.User)
            .Include(t => t.Categories)
            .Include(t => t.PortfolioItems)
            .FirstOrDefaultAsync(cancellationToken);

        if (tailor is null)
            return Result.Failure<TailorDetailsResponse>(TailorErrors.TailorNotFound);

        return Result.Success(tailor.Adapt<TailorDetailsResponse>());
    }
}
