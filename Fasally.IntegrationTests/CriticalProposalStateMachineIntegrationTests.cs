using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Measurements;
using Fasally.Contracts.Proposals;
using Fasally.Entities.Enums;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class CriticalProposalStateMachineIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task FinalizeBeforeAccept_Fails_AndKeepsProposalPending()
    {
        var setup = await CreateProposalSetupAsync("finalize-before-accept");

        var response = await setup.TailorClient.PostAsJsonAsync($"/api/tailor/proposals/{setup.ProposalId}/finalize", FinalizeRequest(setup.ProductId));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Where(p => p.Id == setup.ProposalId)
                .Select(p => new { p.Status, p.AcceptedAt, ProductCount = p.Products.Count, p.TotalPrice })
                .SingleAsync());

        Assert.Equal(ProposalStatus.Pending, state.Status);
        Assert.Null(state.AcceptedAt);
        Assert.Equal(0, state.ProductCount);
        Assert.Null(state.TotalPrice);
    }

    [Fact]
    public async Task RejectThenFinalize_Fails_AndDoesNotAddProducts()
    {
        var setup = await CreateProposalSetupAsync("reject-then-finalize");

        var reject = await setup.TailorClient.PostAsync($"/api/tailor/proposals/{setup.ProposalId}/reject", null);
        reject.EnsureSuccessStatusCode();

        var finalize = await setup.TailorClient.PostAsJsonAsync($"/api/tailor/proposals/{setup.ProposalId}/finalize", FinalizeRequest(setup.ProductId));

        Assert.Equal(HttpStatusCode.BadRequest, finalize.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Where(p => p.Id == setup.ProposalId)
                .Select(p => new { p.Status, ProductCount = p.Products.Count, p.TotalPrice })
                .SingleAsync());

        Assert.Equal(ProposalStatus.Rejected, state.Status);
        Assert.Equal(0, state.ProductCount);
        Assert.Null(state.TotalPrice);
    }

    [Fact]
    public async Task RejectThenApprove_Fails_AndDoesNotCreateProject()
    {
        var setup = await CreateProposalSetupAsync("reject-then-approve");

        var reject = await setup.Client.PostAsync($"/api/Proposals/{setup.ProposalId}/reject", null);
        reject.EnsureSuccessStatusCode();

        var approve = await setup.Client.PostAsJsonAsync($"/api/Proposals/{setup.ProposalId}/approve", ApproveRequest());

        Assert.Equal(HttpStatusCode.BadRequest, approve.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Where(p => p.Id == setup.ProposalId)
                .Select(p => new { p.Status, ProjectExists = p.Project != null })
                .SingleAsync());

        Assert.Equal(ProposalStatus.Rejected, state.Status);
        Assert.False(state.ProjectExists);
    }

    [Fact]
    public async Task DoubleApproval_Fails_AndCreatesOnlyOneProject()
    {
        var setup = await CreateProposalSetupAsync("double-approval");
        var accept = await setup.TailorClient.PostAsync($"/api/tailor/proposals/{setup.ProposalId}/accept", null);
        accept.EnsureSuccessStatusCode();
        await FinalizeProposalAsync(setup.TailorClient, setup.ProposalId, setup.ProductId);
        var firstProjectId = await ApproveProposalAsync(setup.Client, setup.ProposalId);

        var secondApprove = await setup.Client.PostAsJsonAsync($"/api/Proposals/{setup.ProposalId}/approve", ApproveRequest());

        Assert.Equal(HttpStatusCode.BadRequest, secondApprove.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context => new
        {
            ProposalStatus = await context.Proposals.Where(p => p.Id == setup.ProposalId).Select(p => p.Status).SingleAsync(),
            ProjectIds = await context.Projects.Where(p => p.ProposalId == setup.ProposalId).Select(p => p.Id).ToListAsync()
        });

        Assert.Equal(ProposalStatus.Approved, state.ProposalStatus);
        Assert.Single(state.ProjectIds);
        Assert.Equal(firstProjectId, state.ProjectIds.Single());
    }

    [Fact]
    public async Task WrongClientApproval_Fails_AndDoesNotCreateProject()
    {
        var setup = await CreateProposalSetupAsync("wrong-client-approval");
        using var otherClient = await CreateClientClientAsync(TestData.Email("other-client"));
        var accept = await setup.TailorClient.PostAsync($"/api/tailor/proposals/{setup.ProposalId}/accept", null);
        accept.EnsureSuccessStatusCode();
        await FinalizeProposalAsync(setup.TailorClient, setup.ProposalId, setup.ProductId);

        var approve = await otherClient.PostAsJsonAsync($"/api/Proposals/{setup.ProposalId}/approve", ApproveRequest());

        Assert.Equal(HttpStatusCode.Forbidden, approve.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Where(p => p.Id == setup.ProposalId)
                .Select(p => new { p.Status, ProjectExists = p.Project != null })
                .SingleAsync());

        Assert.Equal(ProposalStatus.AwaitingClientApproval, state.Status);
        Assert.False(state.ProjectExists);
    }

    [Fact]
    public async Task CreateProposal_WithInvalidTailorId_Fails()
    {
        using var client = await CreateClientClientAsync(TestData.Email("invalid-tailor-client"));

        var response = await client.PostAsJsonAsync("/api/Proposals", new CreateProposalRequest
        {
            TailorId = Guid.NewGuid().ToString(),
            Description = "Invalid tailor",
            ImageUrls = []
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var exists = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals.AnyAsync(p => p.Description == "Invalid tailor"));

        Assert.False(exists);
    }

    [Fact]
    public async Task WrongTailorFinalize_Fails_AndDoesNotMutateProposal()
    {
        var setup = await CreateProposalSetupAsync("wrong-tailor-finalize");
        var (otherTailorClient, _, _) = await CreateReadyTailorAsync(setup.CategoryId);
        var accept = await setup.TailorClient.PostAsync($"/api/tailor/proposals/{setup.ProposalId}/accept", null);
        accept.EnsureSuccessStatusCode();

        var finalize = await otherTailorClient.PostAsJsonAsync($"/api/tailor/proposals/{setup.ProposalId}/finalize", FinalizeRequest(setup.ProductId));

        Assert.Equal(HttpStatusCode.Forbidden, finalize.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Where(p => p.Id == setup.ProposalId)
                .Select(p => new { p.Status, ProductCount = p.Products.Count, p.TotalPrice })
                .SingleAsync());

        Assert.Equal(ProposalStatus.Pending, state.Status);
        Assert.Equal(0, state.ProductCount);
        Assert.Null(state.TotalPrice);
    }

    private async Task<ProposalSetup> CreateProposalSetupAsync(string emailPrefix)
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId);
        var client = await CreateClientClientAsync(TestData.Email(emailPrefix));
        var proposalId = await CreateProposalAsync(client, tailorId);

        return new ProposalSetup(categoryId, client, tailorClient, tailorId, productId, proposalId);
    }

    private static FinalizeProposalRequest FinalizeRequest(Guid productId) =>
        new()
        {
            TotalPrice = 1200m,
            ProductionDeadline = DateTime.UtcNow.AddDays(14),
            Products = [new ProposalProductRequest { ProductId = productId, Quantity = 1 }]
        };

    private static ApproveProposalRequest ApproveRequest() =>
        new()
        {
            Measurement = new MeasurementDto { Chest = 40, Waist = 32 },
            UpdateProfileMeasurement = false
        };

    private sealed record ProposalSetup(
        int CategoryId,
        HttpClient Client,
        HttpClient TailorClient,
        string TailorId,
        Guid ProductId,
        Guid ProposalId);
}
