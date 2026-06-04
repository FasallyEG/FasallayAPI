using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Addresses;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class AddressIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Address_Crud_Works_AndPersists()
    {
        var email = TestData.Email("address");
        using var client = await CreateClientClientAsync(email);
        var userId = await GetUserIdAsync(email);

        var create = await client.PostAsJsonAsync("/api/Addresses", Address("Home"));
        create.EnsureSuccessStatusCode();
        var addressId = await create.Content.ReadFromJsonAsync<Guid>();

        var all = await client.GetAsync("/api/Addresses");
        all.EnsureSuccessStatusCode();

        var single = await client.GetAsync($"/api/Addresses/{addressId}");
        single.EnsureSuccessStatusCode();

        var update = await client.PutAsJsonAsync($"/api/Addresses/{addressId}", Address("Updated Home"));
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var storedName = await Factory.ExecuteDbContextAsync(async context =>
            await context.Addresses.Where(a => a.Id == addressId && a.UserId == userId).Select(a => a.Name).SingleAsync());
        Assert.Equal("Updated Home", storedName);

        var delete = await client.DeleteAsync($"/api/Addresses/{addressId}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    [Fact(Skip = "Address-in-use is not part of the current Proposal/Project phase; delivery/address relationships were removed.")]
    public Task DeleteAddress_InUse_ReturnsBusinessError() => Task.CompletedTask;

    private static CreateAddressRequest Address(string name) =>
        new(name, "Cairo", "Nasr City", "Main Street", "10", "2", "5", "Test notes");
}
