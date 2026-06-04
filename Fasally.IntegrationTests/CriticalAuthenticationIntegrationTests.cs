using System.Net;
using System.Net.Http.Json;
using Fasally.Abstractions.Consts;
using Fasally.Contracts.Authentication;
using Fasally.Entities;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class CriticalAuthenticationIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task RefreshToken_Succeeds_AndIssuesNewAccessToken()
    {
        var email = TestData.Email("refresh-success");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, DefaultRoles.Member);
        var login = await LoginWithResponseAsync(email);

        using var client = AnonymousClient;
        var response = await client.PostAsJsonAsync("/Auth/refresh", new RefreshTokenRequest(login.Token, login.RefreshToken));

        response.EnsureSuccessStatusCode();
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(refreshed);
        Assert.False(string.IsNullOrWhiteSpace(refreshed!.Token));
        Assert.False(string.IsNullOrWhiteSpace(refreshed.RefreshToken));
        Assert.NotEqual(login.RefreshToken, refreshed.RefreshToken);

        var tokenState = await Factory.ExecuteDbContextAsync(async context =>
        {
            var user = await context.Users.SingleAsync(u => u.Email == email);
            return await context.RefreshTokens
                .Where(t => t.UserId == user.Id)
                .Select(t => new { t.Token, t.RevokedOn })
                .ToListAsync();
        });

        Assert.Contains(tokenState, t => t.Token == login.RefreshToken && t.RevokedOn is not null);
        Assert.Contains(tokenState, t => t.Token == refreshed.RefreshToken && t.RevokedOn is null);
    }

    [Fact]
    public async Task RefreshToken_Rotation_PreventsReusingOldRefreshToken()
    {
        var email = TestData.Email("refresh-rotation");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, DefaultRoles.Member);
        var login = await LoginWithResponseAsync(email);

        using var client = AnonymousClient;
        var firstRefresh = await client.PostAsJsonAsync("/Auth/refresh", new RefreshTokenRequest(login.Token, login.RefreshToken));
        firstRefresh.EnsureSuccessStatusCode();

        var reused = await client.PostAsJsonAsync("/Auth/refresh", new RefreshTokenRequest(login.Token, login.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);

        var oldTokenRevoked = await Factory.ExecuteDbContextAsync(async context =>
            await context.RefreshTokens
                .Where(t => t.Token == login.RefreshToken)
                .Select(t => t.RevokedOn != null)
                .SingleAsync());

        Assert.True(oldTokenRevoked);
    }

    [Fact]
    public async Task RefreshToken_Revocation_PreventsFutureRefresh()
    {
        var email = TestData.Email("refresh-revoke");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, DefaultRoles.Member);
        var login = await LoginWithResponseAsync(email);

        using var client = AnonymousClient;
        var revoke = await client.PostAsJsonAsync("/Auth/revoke-refresh-token", new RefreshTokenRequest(login.Token, login.RefreshToken));
        revoke.EnsureSuccessStatusCode();

        var refresh = await client.PostAsJsonAsync("/Auth/refresh", new RefreshTokenRequest(login.Token, login.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);

        var revoked = await Factory.ExecuteDbContextAsync(async context =>
            await context.RefreshTokens
                .Where(t => t.Token == login.RefreshToken)
                .Select(t => t.RevokedOn != null)
                .SingleAsync());

        Assert.True(revoked);
    }

    [Fact]
    public async Task DisabledUser_CannotLogin()
    {
        var email = TestData.Email("disabled-login");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, DefaultRoles.Member);
        var userId = await GetUserIdAsync(email);
        using var admin = await CreateAdminClientAsync();

        var disable = await admin.PutAsync($"/api/Users/toggle-status/{userId}", null);
        disable.EnsureSuccessStatusCode();

        using var client = AnonymousClient;
        var login = await client.PostAsJsonAsync("/Auth", new LoginRequest(email, TestData.Password));

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);

        var isDisabled = await Factory.ExecuteDbContextAsync(async context =>
            await context.Users.Where(u => u.Id == userId).Select(u => u.IsDisabled).SingleAsync());

        Assert.True(isDisabled);
    }

    [Fact]
    public async Task DisabledUser_CannotRefreshToken()
    {
        var email = TestData.Email("disabled-refresh");
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, DefaultRoles.Member);
        var login = await LoginWithResponseAsync(email);
        var userId = await GetUserIdAsync(email);
        using var admin = await CreateAdminClientAsync();

        var disable = await admin.PutAsync($"/api/Users/toggle-status/{userId}", null);
        disable.EnsureSuccessStatusCode();

        using var client = AnonymousClient;
        var refresh = await client.PostAsJsonAsync("/Auth/refresh", new RefreshTokenRequest(login.Token, login.RefreshToken));

        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);

        var activeTokenStillStored = await Factory.ExecuteDbContextAsync(async context =>
            await context.RefreshTokens.AnyAsync(t => t.UserId == userId && t.Token == login.RefreshToken && t.RevokedOn == null));

        Assert.True(activeTokenStillStored);
    }
}
