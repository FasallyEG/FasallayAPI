using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fasally.Contracts.Authentication;
using Fasally.IntegrationTests.Infrastructure;

namespace Fasally.IntegrationTests;

public sealed class AuthIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Register_Succeeds()
    {
        var user = await RegisterUserAsync(TestData.Email("register"));

        Assert.Equal(HttpStatusCode.OK, user.RegisterResponse.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var email = TestData.Email("duplicate");
        await RegisterUserAsync(email);

        var duplicate = await RegisterUserAsync(email);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.RegisterResponse.StatusCode);
    }

    [Fact]
    public async Task Login_Succeeds_ForConfirmedUser()
    {
        var email = TestData.Email("login");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, "Member");

        var token = await LoginAsync(email);

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var email = TestData.Email("bad-password");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, "Member");

        using var client = AnonymousClient;
        var response = await client.PostAsJsonAsync("/Auth", new LoginRequest(email, "Wrong@12345"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var client = AnonymousClient;

        var response = await client.GetAsync("/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        using var client = AnonymousClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        var response = await client.GetAsync("/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
