using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Tailors;
using Fasally.Contracts.Users;
using Fasally.Entities.Enums;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class TailorIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Admin_Can_Approve_And_Reject_Tailor_Upgrade()
    {
        using var admin = await CreateAdminClientAsync();
        using var member = await CreateClientClientAsync(TestData.Email("tailor-approval"));
        var tailorUserId = await Factory.ExecuteDbContextAsync(async context =>
            await context.Users.Where(u => u.Email!.StartsWith("tailor-approval")).Select(u => u.Id).FirstAsync());

        var requestUpgrade = await member.PostAsJsonAsync("/me/request-upgrade", new RequestUpgradeRequest(ProfileType.Tailor));
        Assert.Equal(HttpStatusCode.OK, requestUpgrade.StatusCode);

        var approve = await admin.PutAsync($"/api/Tailors/{tailorUserId}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approve.StatusCode);

        var isTailor = await Factory.ExecuteDbContextAsync(async context =>
            await context.UserRoles.Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                .AnyAsync(x => x.UserId == tailorUserId && x.Name == "Tailor"));
        Assert.True(isTailor);

        using var rejectMember = await CreateClientClientAsync(TestData.Email("tailor-reject"));
        var rejectUserId = await Factory.ExecuteDbContextAsync(async context =>
            await context.Users.Where(u => u.Email!.StartsWith("tailor-reject")).Select(u => u.Id).FirstAsync());
        await rejectMember.PostAsJsonAsync("/me/request-upgrade", new RequestUpgradeRequest(ProfileType.Tailor));

        var reject = await admin.PutAsync($"/api/Tailors/{rejectUserId}/reject", null);
        Assert.Equal(HttpStatusCode.NoContent, reject.StatusCode);
    }

    [Fact]
    public async Task Tailor_Profile_Update_And_Portfolio_Work()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailor, userId, _) = await CreateReadyTailorAsync(categoryId);

        var update = await tailor.PutAsJsonAsync("/api/Tailors/profile", new UpdateTailorRequest(
            7,
            [categoryId],
            "Updated bio"));
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        await AddPortfolioItemAsync(tailor);

        var portfolio = await tailor.GetAsync("/api/Tailors/my-portfolio");
        portfolio.EnsureSuccessStatusCode();
        var items = await portfolio.Content.ReadFromJsonAsync<List<PortfolioItemResponse>>();
        Assert.NotEmpty(items!);

        var profile = await Factory.ExecuteDbContextAsync(async context =>
            await context.Tailors.Include(t => t.PortfolioItems).SingleAsync(t => t.ApplicationUserId == userId));
        Assert.Equal(7, profile.ExperienceYears);
        Assert.NotEmpty(profile.PortfolioItems);
    }

    [Fact]
    public async Task Member_Cannot_Create_Tailor_Profile_WithoutTailorRole()
    {
        using var member = await CreateClientClientAsync(TestData.Email("not-tailor"));

        var response = await member.PostAsJsonAsync("/api/Tailors/profile", new CreateTailorRequest(
            1,
            null,
            "No role",
            null,
            null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
