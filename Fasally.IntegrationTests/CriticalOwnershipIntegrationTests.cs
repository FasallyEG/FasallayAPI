using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Addresses;
using Fasally.Contracts.Measurements;
using Fasally.Contracts.Products;
using Fasally.Entities.Enums;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class CriticalOwnershipIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task AddressOwnership_PreventsOtherUserReadUpdateDelete()
    {
        using var owner = await CreateClientClientAsync(TestData.Email("address-owner"));
        using var otherUser = await CreateClientClientAsync(TestData.Email("address-other"));

        var create = await owner.PostAsJsonAsync("/api/Addresses", new CreateAddressRequest(
            "Home",
            "Cairo",
            "Nasr City",
            "Main",
            "10",
            "2",
            "4",
            "Owner address"));
        create.EnsureSuccessStatusCode();
        var addressId = await create.Content.ReadFromJsonAsync<Guid>();

        var get = await otherUser.GetAsync($"/api/Addresses/{addressId}");
        var update = await otherUser.PutAsJsonAsync($"/api/Addresses/{addressId}", new UpdateAddressRequest(
            "Stolen",
            "Giza",
            "Dokki",
            "Other",
            "22",
            null,
            null,
            "Should not update"));
        var delete = await otherUser.DeleteAsync($"/api/Addresses/{addressId}");

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);

        var address = await Factory.ExecuteDbContextAsync(async context =>
            await context.Addresses.SingleAsync(a => a.Id == addressId));

        Assert.Equal("Home", address.Name);
        Assert.Equal("Cairo", address.City);
    }

    [Fact]
    public async Task ProposalOwnership_PreventsOtherClientReadAndApprove()
    {
        var setup = await CreateProposalOwnershipSetupAsync();
        using var otherClient = await CreateClientClientAsync(TestData.Email("proposal-other-client"));
        var accept = await setup.TailorClient.PostAsync($"/api/tailor/proposals/{setup.ProposalId}/accept", null);
        accept.EnsureSuccessStatusCode();
        await FinalizeProposalAsync(setup.TailorClient, setup.ProposalId, setup.ProductId);

        var get = await otherClient.GetAsync($"/api/Proposals/{setup.ProposalId}");
        var approve = await otherClient.PostAsJsonAsync($"/api/Proposals/{setup.ProposalId}/approve", new Fasally.Contracts.Proposals.ApproveProposalRequest
        {
            Measurement = new MeasurementDto { Chest = 42 },
            UpdateProfileMeasurement = false
        });

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, approve.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Where(p => p.Id == setup.ProposalId)
                .Select(p => new { p.Status, ProjectCount = context.Projects.Count(project => project.ProposalId == p.Id) })
                .SingleAsync());

        Assert.Equal(ProposalStatus.AwaitingClientApproval, state.Status);
        Assert.Equal(0, state.ProjectCount);
    }

    [Fact]
    public async Task ProjectOwnership_PreventsOtherUserListAndDetailsAccess()
    {
        var setup = await CreateProjectOwnershipSetupAsync();
        using var otherClient = await CreateClientClientAsync(TestData.Email("project-other-client"));

        var list = await otherClient.GetFromJsonAsync<List<Fasally.Contracts.Projects.ProjectListItemResponse>>("/api/Projects");
        var details = await otherClient.GetAsync($"/api/Projects/{setup.ProjectId}");

        Assert.DoesNotContain(list!, p => p.Id == setup.ProjectId);
        Assert.Equal(HttpStatusCode.NotFound, details.StatusCode);

        var exists = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects.AnyAsync(p => p.Id == setup.ProjectId));

        Assert.True(exists);
    }

    [Fact]
    public async Task ProductOwnership_PreventsOtherSellerUpdateInventoryAndVariantManagement()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (sellerAClient, sellerAId, _) = await CreateReadySellerAsync();
        var (sellerBClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerAClient, categoryId, "Owner-only fabric");

        var update = await sellerBClient.PutAsJsonAsync($"/api/Products/{productId}", new UpdateProductRequest(
            "Hijacked fabric",
            "Should not persist",
            99m,
            1,
            categoryId,
            ProductStatus.Active));

        var stock = await sellerBClient.PutAsJsonAsync($"/api/Products/{productId}/stock", new UpdateProductStockRequest(99, "Should not persist"));

        var variant = await sellerBClient.PostAsJsonAsync($"/api/Products/{productId}/variants", new ProductVariantRequest("Color", "Black"));

        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, stock.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, variant.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Products
                .Where(p => p.Id == productId)
                .Select(p => new
                {
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.SellerProfileId,
                    VariantCount = p.Variants.Count,
                    InventoryLogCount = p.InventoryLogs.Count
                })
                .SingleAsync());

        Assert.Equal("Owner-only fabric", state.Name);
        Assert.Equal(250m, state.Price);
        Assert.Equal(20, state.Stock);
        Assert.Equal(sellerAId, state.SellerProfileId);
        Assert.Equal(0, state.VariantCount);
        Assert.Equal(0, state.InventoryLogCount);
    }

    private async Task<ProposalOwnershipSetup> CreateProposalOwnershipSetupAsync()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId);
        var owner = await CreateClientClientAsync(TestData.Email("proposal-owner"));
        var proposalId = await CreateProposalAsync(owner, tailorId);

        return new ProposalOwnershipSetup(owner, tailorClient, productId, proposalId);
    }

    private async Task<ProjectOwnershipSetup> CreateProjectOwnershipSetupAsync()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId);
        var owner = await CreateClientClientAsync(TestData.Email("project-owner"));
        var projectId = await CreateApprovedProjectAsync(owner, tailorClient, tailorId, productId);

        return new ProjectOwnershipSetup(owner, projectId);
    }

    private sealed record ProposalOwnershipSetup(
        HttpClient OwnerClient,
        HttpClient TailorClient,
        Guid ProductId,
        Guid ProposalId);

    private sealed record ProjectOwnershipSetup(HttpClient OwnerClient, Guid ProjectId);
}
