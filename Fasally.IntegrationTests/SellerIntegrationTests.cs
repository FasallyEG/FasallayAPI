using System.Net;
using System.Net.Http.Json;
using Fasally.Contracts.Products;
using Fasally.Contracts.Sellers;
using Fasally.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Fasally.IntegrationTests;

public sealed class SellerIntegrationTests(CustomWebApplicationFactory factory) : DomainTestBase(factory)
{
    [Fact]
    public async Task Seller_Profile_Product_Dashboard_And_Inventory_Work()
    {
        var (seller, sellerId, _) = await CreateReadySellerAsync();

        var me = await seller.GetAsync("/api/Sellers/me");
        me.EnsureSuccessStatusCode();

        var update = await seller.PutAsJsonAsync("/api/Sellers/profile", new UpdateSellerProfileRequest(
            "Updated Store",
            "Updated",
            "01111111111",
            "updated-store@example.test",
            null));
        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

        var productId = await CreateProductAsync(seller, name: "Navy Fabric");

        var addImage = await seller.PostAsJsonAsync($"/api/Products/{productId}/images", new AddProductImageRequest(
            "https://images.example.test/fabric.png",
            "fabric",
            1));
        addImage.EnsureSuccessStatusCode();

        var addVariant = await seller.PostAsJsonAsync($"/api/Products/{productId}/variants", new ProductVariantRequest("Color", "Navy"));
        addVariant.EnsureSuccessStatusCode();
        var variant = await addVariant.Content.ReadFromJsonAsync<ProductVariantResponse>();

        var updateVariant = await seller.PutAsJsonAsync($"/api/Products/{productId}/variants/{variant!.Id}", new ProductVariantRequest("Color", "Midnight"));
        Assert.Equal(HttpStatusCode.NoContent, updateVariant.StatusCode);

        var stock = await seller.PutAsJsonAsync($"/api/Products/{productId}/stock", new UpdateProductStockRequest(12, "Stock correction"));
        Assert.Equal(HttpStatusCode.NoContent, stock.StatusCode);

        var inventory = await seller.GetAsync($"/api/Products/{productId}/inventory");
        inventory.EnsureSuccessStatusCode();
        var inventoryResponse = await inventory.Content.ReadFromJsonAsync<ProductInventoryResponse>();
        Assert.Equal(12, inventoryResponse!.CurrentStock);
        Assert.NotEmpty(inventoryResponse.Logs);

        var products = await seller.GetAsync("/api/Sellers/me/products");
        products.EnsureSuccessStatusCode();

        var dashboard = await seller.GetAsync("/api/Sellers/me/dashboard");
        dashboard.EnsureSuccessStatusCode();
        var dashboardResponse = await dashboard.Content.ReadFromJsonAsync<SellerDashboardResponse>();
        Assert.True(dashboardResponse!.TotalProducts >= 1);

        var persisted = await Factory.ExecuteDbContextAsync(async context =>
            await context.Products.SingleAsync(p => p.Id == productId));
        Assert.Equal(sellerId, persisted.SellerProfileId);
        Assert.Equal(12, persisted.Stock);
    }

    [Fact]
    public async Task Member_Cannot_Create_Product()
    {
        using var member = await CreateClientClientAsync(TestData.Email("not-seller"));

        var response = await member.PostAsJsonAsync("/api/Products", new CreateProductRequest(
            "Forbidden product",
            null,
            10,
            1,
            null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
