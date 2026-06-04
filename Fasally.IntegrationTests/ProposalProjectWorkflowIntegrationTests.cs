using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Measurements;
using Fasally.Contracts.Products;
using Fasally.Contracts.Proposals;
using Fasally.Entities.Enums;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class ProposalProjectWorkflowIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Full_Proposal_To_Project_Workflow_Works()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId, "Navy Wool");
        var clientEmail = TestData.Email("proposal-client");
        using var client = await CreateClientClientAsync(clientEmail);
        var clientId = await GetUserIdAsync(clientEmail);

        var createProposal = await client.PostAsJsonAsync("/api/Proposals", new CreateProposalRequest
        {
            TailorId = tailorId,
            Description = "I want a classic navy suit similar to these images.",
            ResponseDeadline = DateTime.UtcNow.AddDays(3),
            ImageUrls = ["https://images.example.test/reference-1.png"]
        });
        createProposal.EnsureSuccessStatusCode();
        var proposalId = await createProposal.Content.ReadFromJsonAsync<Guid>();

        var createdStatus = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals.Where(p => p.Id == proposalId).Select(p => p.Status).SingleAsync());
        Assert.Equal(ProposalStatus.Pending, createdStatus);

        var accept = await tailorClient.PostAsync($"/api/tailor/proposals/{proposalId}/accept", null);
        Assert.Equal(HttpStatusCode.NoContent, accept.StatusCode);

        var finalize = await tailorClient.PostAsJsonAsync($"/api/tailor/proposals/{proposalId}/finalize", new FinalizeProposalRequest
        {
            TotalPrice = 1500m,
            ProductionDeadline = DateTime.UtcNow.AddDays(21),
            Products =
            [
                new ProposalProductRequest
                {
                    ProductId = productId,
                    Quantity = 2
                }
            ]
        });
        Assert.Equal(HttpStatusCode.NoContent, finalize.StatusCode);

        var finalized = await Factory.ExecuteDbContextAsync(async context =>
            await context.Proposals
                .Include(p => p.Products)
                .SingleAsync(p => p.Id == proposalId));
        Assert.Equal(ProposalStatus.AwaitingClientApproval, finalized.Status);
        Assert.Equal(1500m, finalized.TotalPrice);
        Assert.Single(finalized.Products);
        Assert.Equal(250m, finalized.Products.Single().UnitPriceAtProposal);

        var approve = await client.PostAsJsonAsync($"/api/Proposals/{proposalId}/approve", new ApproveProposalRequest
        {
            UpdateProfileMeasurement = true,
            Measurement = new MeasurementDto
            {
                Chest = 41,
                Waist = 34,
                Hip = 42,
                Length = 73,
                Sleeve = 62,
                AdditionalMeasurements = "Snapshot source"
            }
        });
        approve.EnsureSuccessStatusCode();
        var projectId = await approve.Content.ReadFromJsonAsync<Guid>();

        var state = await Factory.ExecuteDbContextAsync(async context =>
        {
            var proposal = await context.Proposals.SingleAsync(p => p.Id == proposalId);
            var project = await context.Projects
                .Include(p => p.MeasurementSnapshot)
                .SingleAsync(p => p.Id == projectId);
            var profileMeasurement = await context.ClientMeasurements.SingleAsync(m => m.ApplicationUserId == clientId);
            return new
            {
                ProposalStatus = proposal.Status,
                ProjectStatus = project.Status,
                SnapshotChest = project.MeasurementSnapshot.Chest,
                ProfileChest = profileMeasurement.Chest,
                project.ProposalId
            };
        });

        Assert.Equal(ProposalStatus.Approved, state.ProposalStatus);
        Assert.Equal(ProjectStatus.Approved, state.ProjectStatus);
        Assert.Equal(41, state.SnapshotChest);
        Assert.Equal(41, state.ProfileChest);
        Assert.Equal(proposalId, state.ProposalId);

        var projects = await client.GetAsync("/api/Projects");
        projects.EnsureSuccessStatusCode();

        var details = await client.GetAsync($"/api/Projects/{projectId}");
        details.EnsureSuccessStatusCode();

        var start = await tailorClient.PostAsync($"/api/Projects/{projectId}/start", null);
        Assert.Equal(HttpStatusCode.NoContent, start.StatusCode);

        var inProgress = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects.Where(p => p.Id == projectId).Select(p => p.Status).SingleAsync());
        Assert.Equal(ProjectStatus.InProgress, inProgress);

        var complete = await tailorClient.PostAsync($"/api/Projects/{projectId}/complete", null);
        Assert.Equal(HttpStatusCode.NoContent, complete.StatusCode);

        var completed = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects.Where(p => p.Id == projectId).Select(p => new { p.Status, p.CompletedAt }).SingleAsync());
        Assert.Equal(ProjectStatus.Completed, completed.Status);
        Assert.NotNull(completed.CompletedAt);
    }

    [Fact]
    public async Task WrongRole_Cannot_Start_Project()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId);
        using var client = await CreateClientClientAsync(TestData.Email("wrong-project-role-client"));
        using var otherMember = await CreateClientClientAsync(TestData.Email("wrong-project-role-other"));

        var proposalId = await CreateApprovedProposalAsync(client, tailorClient, tailorId, productId);

        var projectId = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects.Where(p => p.ProposalId == proposalId).Select(p => p.Id).SingleAsync());

        var response = await otherMember.PostAsync($"/api/Projects/{projectId}/start", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<Guid> CreateApprovedProposalAsync(HttpClient client, HttpClient tailor, string tailorId, Guid productId)
    {
        var create = await client.PostAsJsonAsync("/api/Proposals", new CreateProposalRequest
        {
            TailorId = tailorId,
            Description = "A proposal to approve",
            ImageUrls = []
        });
        create.EnsureSuccessStatusCode();
        var proposalId = await create.Content.ReadFromJsonAsync<Guid>();

        var accept = await tailor.PostAsync($"/api/tailor/proposals/{proposalId}/accept", null);
        accept.EnsureSuccessStatusCode();

        var finalize = await tailor.PostAsJsonAsync($"/api/tailor/proposals/{proposalId}/finalize", new FinalizeProposalRequest
        {
            TotalPrice = 1000m,
            ProductionDeadline = DateTime.UtcNow.AddDays(10),
            Products = [new ProposalProductRequest { ProductId = productId, Quantity = 1 }]
        });
        finalize.EnsureSuccessStatusCode();

        var approve = await client.PostAsJsonAsync($"/api/Proposals/{proposalId}/approve", new ApproveProposalRequest
        {
            Measurement = new MeasurementDto { Chest = 40 },
            UpdateProfileMeasurement = false
        });
        approve.EnsureSuccessStatusCode();

        return proposalId;
    }
}
