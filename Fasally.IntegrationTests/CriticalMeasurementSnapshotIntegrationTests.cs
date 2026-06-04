using System.Net.Http.Json;
using Fasally.Contracts.Measurements;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class CriticalMeasurementSnapshotIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task ProjectMeasurementSnapshot_RemainsUnchanged_WhenProfileMeasurementChangesLater()
    {
        using var admin = await CreateAdminClientAsync();
        var categoryId = await CreateCategoryAsync(admin);
        var (tailorClient, tailorId, _) = await CreateReadyTailorAsync(categoryId);
        var (sellerClient, _, _) = await CreateReadySellerAsync();
        var productId = await CreateProductAsync(sellerClient, categoryId);
        var clientEmail = TestData.Email("snapshot-client");
        using var client = await CreateClientClientAsync(clientEmail);
        var clientId = await GetUserIdAsync(clientEmail);

        var original = new MeasurementDto
        {
            Chest = 40,
            Waist = 32,
            Hip = 41,
            Length = 72,
            Sleeve = 61,
            AdditionalMeasurements = "Original approval measurements"
        };

        var projectId = await CreateApprovedProjectAsync(
            client,
            tailorClient,
            tailorId,
            productId,
            original,
            updateProfileMeasurement: true);

        var updatedProfile = new MeasurementDto
        {
            Chest = 47,
            Waist = 38,
            Hip = 49,
            Length = 75,
            Sleeve = 65,
            AdditionalMeasurements = "Updated profile only"
        };

        var update = await client.PutAsJsonAsync("/api/Measurements/me", updatedProfile);
        update.EnsureSuccessStatusCode();

        var state = await Factory.ExecuteDbContextAsync(async context =>
        {
            var project = await context.Projects
                .Include(p => p.MeasurementSnapshot)
                .SingleAsync(p => p.Id == projectId);
            var profile = await context.ClientMeasurements.SingleAsync(m => m.ApplicationUserId == clientId);

            return new
            {
                SnapshotChest = project.MeasurementSnapshot.Chest,
                SnapshotWaist = project.MeasurementSnapshot.Waist,
                SnapshotNotes = project.MeasurementSnapshot.AdditionalMeasurements,
                ProfileChest = profile.Chest,
                ProfileWaist = profile.Waist,
                ProfileNotes = profile.AdditionalMeasurements
            };
        });

        Assert.Equal(original.Chest, state.SnapshotChest);
        Assert.Equal(original.Waist, state.SnapshotWaist);
        Assert.Equal(original.AdditionalMeasurements, state.SnapshotNotes);
        Assert.Equal(updatedProfile.Chest, state.ProfileChest);
        Assert.Equal(updatedProfile.Waist, state.ProfileWaist);
        Assert.Equal(updatedProfile.AdditionalMeasurements, state.ProfileNotes);
    }
}
