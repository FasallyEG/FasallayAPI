using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Measurements;
using Fasally.Contracts.Users;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class AccountAndMeasurementIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Account_Profile_Update_And_ChangePassword_Work()
    {
        var email = TestData.Email("account");
        using var client = await CreateClientClientAsync(email);

        var profile = await client.GetAsync("/me");
        profile.EnsureSuccessStatusCode();

        var update = await client.PutAsJsonAsync("/me/info", new UpdateProfileRequest("Updated", "Member"));
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var passwordChange = await client.PutAsJsonAsync("/me/change-password", new ChangePasswordRequest(TestData.Password, "NewPassword@12345"));
        Assert.Equal(HttpStatusCode.NoContent, passwordChange.StatusCode);

        var token = await LoginAsync(email, "NewPassword@12345");
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Measurements_Save_Update_Get_AndPersist()
    {
        var email = TestData.Email("measurements");
        var userId = await GetUserIdAfterClientCreation(email);
        using var client = await CreateAuthorizedClientFromSeededUserAsync(email, TestData.Password);

        var save = await client.PutAsJsonAsync("/api/Measurements/me", new MeasurementDto
        {
            Chest = 40,
            Waist = 32,
            Hip = 41,
            Length = 72,
            Sleeve = 61,
            AdditionalMeasurements = "Initial"
        });
        save.EnsureSuccessStatusCode();

        var update = await client.PutAsJsonAsync("/api/Measurements/me", new MeasurementDto
        {
            Chest = 42,
            Waist = 33,
            AdditionalMeasurements = "Updated"
        });
        update.EnsureSuccessStatusCode();

        var get = await client.GetAsync("/api/Measurements/me");
        get.EnsureSuccessStatusCode();
        var measurement = await get.Content.ReadFromJsonAsync<MeasurementDto>();
        Assert.Equal(42, measurement!.Chest);
        Assert.Equal("Updated", measurement.AdditionalMeasurements);

        var stored = await Factory.ExecuteDbContextAsync(async context =>
            await context.ClientMeasurements.SingleAsync(m => m.ApplicationUserId == userId));
        Assert.Equal(42, stored.Chest);
    }

    private async Task<string> GetUserIdAfterClientCreation(string email)
    {
        await RegisterUserAsync(email);
        await ConfirmUserAndAssignRoleAsync(email, "Member");
        return await GetUserIdAsync(email);
    }
}
