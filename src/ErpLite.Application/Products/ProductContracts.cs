using ErpLite.Domain.Products;

namespace ErpLite.Application.Products;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    decimal UnitPrice,
    int QuantityOnHand,
    bool IsActive)
{
    public static ProductDto From(Product p) =>
        new(p.Id, p.Sku, p.Name, p.UnitPrice, p.QuantityOnHand, p.IsActive);
}

public sealed record CreateProductRequest(string Sku, string Name, decimal UnitPrice);

public sealed record UpdateProductRequest(string Name, decimal UnitPrice, bool IsActive);

public sealed record StockMovementRequest(int Quantity);

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAsync(string? search, CancellationToken ct);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct);
    void Add(Product product);
    Task SaveChangesAsync(CancellationToken ct);
}
