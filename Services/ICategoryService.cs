using Fasally.Abstractions;
using Fasally.Contracts.Categories;

namespace Fasally.Services;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryResponse>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<CategoryResponse>> GetAsync(int id, CancellationToken cancellationToken = default);

    Task<Result<CategoryResponse>> AddAsync(CategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
