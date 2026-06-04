using Fasally.Abstractions;
using Fasally.Contracts.Categories;
using Fasally.Entities;
using Fasally.Errors;
using Fasally.Persistence;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Fasally.Services;

public class CategoryService(ApplicationDbContext context) : ICategoryService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<IEnumerable<CategoryResponse>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories
            .ProjectToType<CategoryResponse>()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<Result<CategoryResponse>> GetAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .Where(c => c.Id == id)
            .ProjectToType<CategoryResponse>()
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
            return Result.Failure<CategoryResponse>(CategoryErrors.CategoryNotFound);

        return Result.Success(category);
    }

    public async Task<Result<CategoryResponse>> AddAsync(
        CategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var isDuplicate = await _context.Categories
            .AnyAsync(c => c.Name == request.Name, cancellationToken);

        if (isDuplicate)
            return Result.Failure<CategoryResponse>(CategoryErrors.DuplicatedCategory);

        var category = request.Adapt<Category>();

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Adapt<CategoryResponse>());
    }

    public async Task<Result> UpdateAsync(
        int id,
        CategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
            return Result.Failure(CategoryErrors.CategoryNotFound);

        var isDuplicate = await _context.Categories
            .AnyAsync(c => c.Name == request.Name && c.Id != id, cancellationToken);

        if (isDuplicate)
            return Result.Failure(CategoryErrors.DuplicatedCategory);

        request.Adapt(category);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category is null)
            return Result.Failure(CategoryErrors.CategoryNotFound);

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
