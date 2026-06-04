using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Categories;
using Fasally.Contracts.Measurements;
using Fasally.Contracts.Products;
using Fasally.Contracts.Proposals;
using Fasally.Contracts.Sellers;
using Fasally.Contracts.Tailors;
using Fasally.Entities.Enums;

namespace Fasally.IntegrationTests.Infrastructure;

public abstract partial class DomainTestBase(CustomWebApplicationFactory factory) : ApiTestBase(factory)
{
    protected async Task<int> CreateCategoryAsync(HttpClient adminClient, string? name = null)
    {
        var response = await adminClient.PostAsJsonAsync("/api/Categories", new CategoryRequest(name ?? $"Category {Guid.NewGuid():N}"));
        response.EnsureSuccessStatusCode();
        var category = await response.Content.ReadFromJsonAsync<CategoryResponse>();
        return category!.Id;
    }

    protected async Task CreateTailorProfileAsync(HttpClient tailorClient, int categoryId)
    {
        var response = await tailorClient.PostAsJsonAsync("/api/Tailors/profile", new CreateTailorRequest(
            5,
            [categoryId],
            "Experienced tailor",
            "https://images.example.test/national.png",
            "https://images.example.test/shop.png"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    protected async Task AddPortfolioItemAsync(HttpClient tailorClient)
    {
        var response = await tailorClient.PostAsJsonAsync("/api/Tailors/my-portfolio", new CreatePortfolioItemRequest(
            "Classic Suit",
            "A navy suit",
            ["https://images.example.test/suit.png"]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    protected async Task CreateSellerProfileAsync(HttpClient sellerClient)
    {
        var response = await sellerClient.PostAsJsonAsync("/api/Sellers/profile", new CreateSellerProfileRequest(
            $"Store {Guid.NewGuid():N}",
            "Fabric store",
            "01000000000",
            $"seller-{Guid.NewGuid():N}@example.test",
            "https://images.example.test/store.png"));

        response.EnsureSuccessStatusCode();
    }

    protected async Task<Guid> CreateProductAsync(HttpClient sellerClient, int? categoryId = null, string? name = null)
    {
        var response = await sellerClient.PostAsJsonAsync("/api/Products", new CreateProductRequest(
            name ?? $"Fabric {Guid.NewGuid():N}",
            "Premium fabric",
            250m,
            20,
            categoryId,
            ProductStatus.Active));

        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        return product!.Id;
    }

    protected async Task<(HttpClient Client, string UserId, string Email)> CreateReadyTailorAsync(int categoryId)
    {
        var email = TestData.Email("tailor");
        var client = await CreateTailorClientAsync(email);
        await CreateTailorProfileAsync(client, categoryId);
        var userId = await GetUserIdAsync(email);
        return (client, userId, email);
    }

    protected async Task<(HttpClient Client, string UserId, string Email)> CreateReadySellerAsync()
    {
        var email = TestData.Email("seller");
        var client = await CreateSellerClientAsync(email);
        await CreateSellerProfileAsync(client);
        var userId = await GetUserIdAsync(email);
        return (client, userId, email);
    }

    protected async Task<Guid> CreateProposalAsync(HttpClient client, string tailorId, string? description = null)
    {
        var response = await client.PostAsJsonAsync("/api/Proposals", new CreateProposalRequest
        {
            TailorId = tailorId,
            Description = description ?? "A critical workflow proposal",
            ResponseDeadline = DateTime.UtcNow.AddDays(3),
            ImageUrls = ["https://images.example.test/reference.png"]
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    protected async Task FinalizeProposalAsync(HttpClient tailorClient, Guid proposalId, Guid productId)
    {
        var response = await tailorClient.PostAsJsonAsync($"/api/tailor/proposals/{proposalId}/finalize", new FinalizeProposalRequest
        {
            TotalPrice = 1000m,
            ProductionDeadline = DateTime.UtcNow.AddDays(14),
            Products = [new ProposalProductRequest { ProductId = productId, Quantity = 1 }]
        });

        response.EnsureSuccessStatusCode();
    }

    protected async Task<Guid> ApproveProposalAsync(
        HttpClient client,
        Guid proposalId,
        MeasurementDto? measurement = null,
        bool updateProfileMeasurement = false)
    {
        var response = await client.PostAsJsonAsync($"/api/Proposals/{proposalId}/approve", new ApproveProposalRequest
        {
            Measurement = measurement ?? new MeasurementDto { Chest = 40, Waist = 32 },
            UpdateProfileMeasurement = updateProfileMeasurement
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    protected async Task<Guid> CreateApprovedProjectAsync(
        HttpClient client,
        HttpClient tailorClient,
        string tailorId,
        Guid productId,
        MeasurementDto? measurement = null,
        bool updateProfileMeasurement = false)
    {
        var proposalId = await CreateProposalAsync(client, tailorId);

        var accept = await tailorClient.PostAsync($"/api/tailor/proposals/{proposalId}/accept", null);
        accept.EnsureSuccessStatusCode();

        await FinalizeProposalAsync(tailorClient, proposalId, productId);
        return await ApproveProposalAsync(client, proposalId, measurement, updateProfileMeasurement);
    }
}
