using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Categories;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class CategoryIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Category_Crud_Works_AndPersists()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryName = $"Formal {Guid.NewGuid():N}";

        var create = await admin.PostAsJsonAsync("/api/Categories", new CategoryRequest(categoryName));
        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<CategoryResponse>();

        var all = await admin.GetAsync("/api/Categories");
        all.EnsureSuccessStatusCode();
        var categories = await all.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        Assert.Contains(categories!, c => c.Id == created!.Id);

        var update = await admin.PutAsJsonAsync($"/api/Categories/{created!.Id}", new CategoryRequest($"{categoryName} Updated"));
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var persistedName = await Factory.ExecuteDbContextAsync(async context =>
            await context.Categories.Where(c => c.Id == created.Id).Select(c => c.Name).SingleAsync());
        Assert.EndsWith("Updated", persistedName);

        var delete = await admin.DeleteAsync($"/api/Categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var exists = await Factory.ExecuteDbContextAsync(async context =>
            await context.Categories.AnyAsync(c => c.Id == created.Id));
        Assert.False(exists);
    }

    [Fact]
    public async Task CreateCategory_WithoutToken_ReturnsUnauthorized()
    {
        using var client = AnonymousClient;

        var response = await client.PostAsJsonAsync("/api/Categories", new CategoryRequest("Unauthorized"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_WithWrongRole_ReturnsForbidden()
    {
        using var member = await CreateClientClientAsync(TestData.Email("category-member"));

        var response = await member.PostAsJsonAsync("/api/Categories", new CategoryRequest("Forbidden"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
