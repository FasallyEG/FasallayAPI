using System.Net;
using Fasally.Entities.Enums;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class CriticalProjectStateMachineIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task CompleteBeforeStart_Fails_AndProjectRemainsApproved()
    {
        var setup = await CreateProjectSetupAsync();

        var complete = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/complete", null);

        Assert.Equal(HttpStatusCode.BadRequest, complete.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects
                .Where(p => p.Id == setup.ProjectId)
                .Select(p => new { p.Status, p.StartedAt, p.CompletedAt })
                .SingleAsync());

        Assert.Equal(ProjectStatus.Approved, state.Status);
        Assert.Null(state.StartedAt);
        Assert.Null(state.CompletedAt);
    }

    [Fact]
    public async Task StartTwice_Fails_AndKeepsOriginalStart()
    {
        var setup = await CreateProjectSetupAsync();

        var firstStart = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/start", null);
        firstStart.EnsureSuccessStatusCode();
        var startedAt = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects.Where(p => p.Id == setup.ProjectId).Select(p => p.StartedAt).SingleAsync());

        var secondStart = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/start", null);

        Assert.Equal(HttpStatusCode.BadRequest, secondStart.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects
                .Where(p => p.Id == setup.ProjectId)
                .Select(p => new { p.Status, p.StartedAt, p.CompletedAt })
                .SingleAsync());

        Assert.Equal(ProjectStatus.InProgress, state.Status);
        Assert.Equal(startedAt, state.StartedAt);
        Assert.Null(state.CompletedAt);
    }

    [Fact]
    public async Task CompleteTwice_Fails_AndKeepsOriginalCompletion()
    {
        var setup = await CreateProjectSetupAsync();

        var start = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/start", null);
        start.EnsureSuccessStatusCode();
        var firstComplete = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/complete", null);
        firstComplete.EnsureSuccessStatusCode();
        var completedAt = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects.Where(p => p.Id == setup.ProjectId).Select(p => p.CompletedAt).SingleAsync());

        var secondComplete = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/complete", null);

        Assert.Equal(HttpStatusCode.BadRequest, secondComplete.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects
                .Where(p => p.Id == setup.ProjectId)
                .Select(p => new { p.Status, p.CompletedAt })
                .SingleAsync());

        Assert.Equal(ProjectStatus.Completed, state.Status);
        Assert.Equal(completedAt, state.CompletedAt);
    }

    [Fact]
    public async Task WrongTailorStart_Fails_AndProjectRemainsApproved()
    {
        var setup = await CreateProjectSetupAsync();
        var (otherTailorClient, _, _) = await CreateReadyTailorAsync(setup.CategoryId);

        var start = await otherTailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/start", null);

        Assert.Equal(HttpStatusCode.Forbidden, start.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects
                .Where(p => p.Id == setup.ProjectId)
                .Select(p => new { p.Status, p.StartedAt })
                .SingleAsync());

        Assert.Equal(ProjectStatus.Approved, state.Status);
        Assert.Null(state.StartedAt);
    }

    [Fact]
    public async Task WrongTailorComplete_Fails_AndProjectRemainsInProgress()
    {
        var setup = await CreateProjectSetupAsync();
        var start = await setup.TailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/start", null);
        start.EnsureSuccessStatusCode();
        var (otherTailorClient, _, _) = await CreateReadyTailorAsync(setup.CategoryId);

        var complete = await otherTailorClient.PostAsync($"/api/Projects/{setup.ProjectId}/complete", null);

        Assert.Equal(HttpStatusCode.Forbidden, complete.StatusCode);

        var state = await Factory.ExecuteDbContextAsync(async context =>
            await context.Projects
                .Where(p => p.Id == setup.ProjectId)
                .Select(p => new { p.Status, p.CompletedAt })
                .SingleAsync());

        Assert.Equal(ProjectStatus.InProgress, state.Status);
        Assert.Null(state.CompletedAt);
    }

    private async Task<ProjectSetup> CreateProjectSetupAsync()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId);
        var client = await CreateClientClientAsync(TestData.Email("critical-project-client"));
        var projectId = await CreateApprovedProjectAsync(client, tailorClient, tailorId, productId);

        return new ProjectSetup(categoryId, client, tailorClient, projectId);
    }

    private sealed record ProjectSetup(
        int CategoryId,
        HttpClient Client,
        HttpClient TailorClient,
        Guid ProjectId);
}
