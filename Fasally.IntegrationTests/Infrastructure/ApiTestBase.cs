using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fasally.Abstractions.Consts;
using Fasally.Contracts.Authentication;
using Fasally.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Fasally.IntegrationTests.Infrastructure;

public abstract class ApiTestBase(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    protected CustomWebApplicationFactory Factory { get; } = factory;

    protected HttpClient AnonymousClient => Factory.CreateApiClient();

    protected async Task<TestUser> RegisterUserAsync(
        string email,
        string password = TestData.Password,
        string firstName = "Test",
        string lastName = "User")
    {
        using var client = Factory.CreateApiClient();
        var response = await client.PostAsJsonAsync("/Auth/register", new RegisterRequest(
            email,
            password,
            firstName,
            lastName,
            null));

        return new TestUser(email, password, response);
    }

    protected async Task<string> LoginAsync(string email, string password = TestData.Password)
    {
        var auth = await LoginWithResponseAsync(email, password);
        return auth.Token;
    }

    protected async Task<AuthResponse> LoginWithResponseAsync(string email, string password = TestData.Password)
    {
        using var client = Factory.CreateApiClient();
        var response = await client.PostAsJsonAsync("/Auth", new LoginRequest(email, password));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    protected async Task<HttpClient> CreateAuthorizedClientAsync(
        string email,
        string role = DefaultRoles.Member,
        string password = TestData.Password)
    {
        await RegisterUserAsync(email, password);
        await ConfirmUserAndAssignRoleAsync(email, role);
        var token = await LoginAsync(email, password);

        var client = Factory.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected Task<HttpClient> CreateAdminClientAsync() =>
        CreateAuthorizedClientFromSeededUserAsync(DefaultUsers.Admin.Email, DefaultUsers.Admin.Password);

    protected Task<HttpClient> CreateClientClientAsync(string email) =>
        CreateAuthorizedClientAsync(email, DefaultRoles.Member);

    protected Task<HttpClient> CreateTailorClientAsync(string email) =>
        CreateAuthorizedClientAsync(email, DefaultRoles.Tailor);

    protected Task<HttpClient> CreateSellerClientAsync(string email) =>
        CreateAuthorizedClientAsync(email, DefaultRoles.Seller);

    protected async Task<HttpClient> CreateAuthorizedClientFromSeededUserAsync(string email, string password)
    {
        var token = await LoginAsync(email, password);
        var client = Factory.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected async Task ConfirmUserAndAssignRoleAsync(string email, string role)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);

        user!.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);
    }

    protected async Task ConfirmUserAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        Assert.NotNull(user);

        user!.EmailConfirmed = true;
        await userManager.UpdateAsync(user);
    }

    protected async Task<string> GetUserIdAsync(string email) =>
        await Factory.ExecuteDbContextAsync(async context =>
            (await context.Users.SingleAsync(u => u.Email == email)).Id);
}

public sealed record TestUser(string Email, string Password, HttpResponseMessage RegisterResponse);

public static class TestData
{
    public const string Password = "Password@12345";

    public static string Email(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}@example.test";
}
