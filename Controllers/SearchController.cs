using Fasally.Abstractions;
using Fasally.Contracts.Search;
using Fasally.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fasally.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class SearchController(ISearchService searchService) : ControllerBase
{
    private readonly ISearchService _searchService = searchService;

    [HttpGet("products")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] ProductSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchProductsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("tailors")]
    public async Task<IActionResult> SearchTailors(
        [FromQuery] TailorSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchTailorsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters(CancellationToken cancellationToken)
    {
        var result = await _searchService.GetFiltersAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }

    [HttpGet("suggestions")]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] SearchSuggestionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _searchService.GetSuggestionsAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }
}
