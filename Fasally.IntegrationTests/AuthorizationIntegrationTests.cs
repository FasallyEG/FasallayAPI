using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Proposals;
using Fasally.IntegrationTests.Infrastructure;

namespace Fasally.IntegrationTests;

public sealed class AuthorizationIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Theory]
    [InlineData("GET", "/api/Proposals")]
    [InlineData("GET", "/api/tailor/proposals")]
    [InlineData("GET", "/api/Projects")]
    [InlineData("GET", "/api/Measurements/me")]
    [InlineData("POST", "/api/Categories")]
    public async Task ProtectedEndpoints_WithoutToken_ReturnUnauthorized(string method, string url)
    {
        using var client = AnonymousClient;
        var response = method == "GET"
            ? await client.GetAsync(url)
            : await client.PostAsJsonAsync(url, new { name = "Unauthorized" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_WithNoPermissionClaim_ReturnsForbidden()
    {
        var email = TestData.Email("no-claims");
        await RegisterUserAsync(email);
        await ConfirmUserAsync(email);
        var token = await LoginAsync(email);
        using var client = Factory.CreateApiClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/Proposals", new CreateProposalRequest
        {
            TailorId = Guid.NewGuid().ToString(),
            Description = "No permission",
            ImageUrls = []
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
