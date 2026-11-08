using System.Net;
using System.Net.Http.Json;
using ErpLite.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace ErpLite.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class ProductsApiTests(ErpLiteApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // Each test uses its own SKU so tests stay independent on a shared database.
    private static string NewSku() => $"T-{Guid.NewGuid():N}"[..20];

    private async Task<ProductDto> CreateAsync(string? sku = null, decimal price = 10m)
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(sku ?? NewSku(), "Test product", price));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy_database()
    {
        var response = await _client.GetAsync(new Uri("/health", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_then_get_returns_the_product()
    {
        var created = await CreateAsync(price: 12.5m);

        var fetched = await _client.GetFromJsonAsync<ProductDto>($"/api/products/{created.Id}");

        Assert.NotNull(fetched);
        Assert.Equal(created.Sku, fetched.Sku);
        Assert.Equal(12.5m, fetched.UnitPrice);
        Assert.Equal(0, fetched.QuantityOnHand);
    }

    [Fact]
    public async Task Duplicate_sku_returns_409_problem_details()
    {
        var sku = NewSku();
        await CreateAsync(sku);

        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(sku.ToLowerInvariant(), "Other", 1m));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Contains("already exists", problem!.Title, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stock_receive_and_issue_update_quantity()
    {
        var product = await CreateAsync();

        await _client.PostAsJsonAsync($"/api/products/{product.Id}/stock/receive", new StockMovementRequest(10));
        var response = await _client.PostAsJsonAsync($"/api/products/{product.Id}/stock/issue", new StockMovementRequest(4));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal(6, updated!.QuantityOnHand);
    }

    [Fact]
    public async Task Issuing_more_than_on_hand_returns_400_and_keeps_stock()
    {
        var product = await CreateAsync();
        await _client.PostAsJsonAsync($"/api/products/{product.Id}/stock/receive", new StockMovementRequest(3));

        var response = await _client.PostAsJsonAsync($"/api/products/{product.Id}/stock/issue", new StockMovementRequest(5));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var fetched = await _client.GetFromJsonAsync<ProductDto>($"/api/products/{product.Id}");
        Assert.Equal(3, fetched!.QuantityOnHand);
    }

    [Fact]
    public async Task Unknown_product_returns_404()
    {
        var response = await _client.GetAsync(new Uri($"/api/products/{Guid.NewGuid()}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Malformed_json_returns_400_not_500()
    {
        using var content = new StringContent("{oops", System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(new Uri("/api/products", UriKind.Relative), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_treats_wildcards_as_plain_text()
    {
        await CreateAsync();

        var results = await _client.GetFromJsonAsync<List<ProductDto>>("/api/products?search=%25");

        Assert.NotNull(results);
        Assert.Empty(results);
    }
}
