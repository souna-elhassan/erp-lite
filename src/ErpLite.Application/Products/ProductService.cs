using ErpLite.Application.Common;
using ErpLite.Domain.Common;
using ErpLite.Domain.Products;

namespace ErpLite.Application.Products;

/// <summary>
/// Product use cases. Business rules live in <see cref="Product"/>; this service orchestrates
/// loading, rule checks that need the database (unique SKU) and saving.
/// </summary>
public sealed class ProductService(IProductRepository products)
{
    public async Task<IReadOnlyList<ProductDto>> ListAsync(string? search, CancellationToken ct)
    {
        var items = await products.ListAsync(search, ct);
        return [.. items.Select(ProductDto.From)];
    }

    public async Task<Result<ProductDto>> GetAsync(Guid id, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null ? NotFound(id) : ProductDto.From(product);
    }

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        Product product;
        try
        {
            product = Product.Create(request.Sku, request.Name, request.UnitPrice);
        }
        catch (DomainException ex)
        {
            return Error.Validation(ex.Message);
        }

        if (await products.SkuExistsAsync(product.Sku, ct))
            return Error.Conflict($"A product with SKU '{product.Sku}' already exists.");

        products.Add(product);
        await products.SaveChangesAsync(ct);
        return ProductDto.From(product);
    }

    public Task<Result<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MutateAsync(id, p =>
        {
            p.Rename(request.Name);
            p.ChangePrice(request.UnitPrice);
            if (request.IsActive) p.Activate(); else p.Deactivate();
        }, ct);
    }

    public Task<Result<ProductDto>> ReceiveStockAsync(Guid id, StockMovementRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MutateAsync(id, p => p.ReceiveStock(request.Quantity), ct);
    }

    public Task<Result<ProductDto>> IssueStockAsync(Guid id, StockMovementRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MutateAsync(id, p => p.IssueStock(request.Quantity), ct);
    }

    private async Task<Result<ProductDto>> MutateAsync(Guid id, Action<Product> change, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        if (product is null) return NotFound(id);

        try
        {
            change(product);
        }
        catch (DomainException ex)
        {
            return Error.Validation(ex.Message);
        }

        await products.SaveChangesAsync(ct);
        return ProductDto.From(product);
    }

    private static Error NotFound(Guid id) => Error.NotFound($"Product '{id}' was not found.");
}
