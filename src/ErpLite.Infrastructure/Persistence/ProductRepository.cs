using ErpLite.Application.Products;
using ErpLite.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace ErpLite.Infrastructure.Persistence;

internal sealed class ProductRepository(ErpLiteDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListAsync(string? search, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = EscapeLike(search.Trim());
            // Prefix search on SKU stays sargable (index seek); name search is a contains.
            query = query.Where(p =>
                EF.Functions.Like(p.Sku, term + "%", "\\") ||
                EF.Functions.Like(p.Name, "%" + term + "%", "\\"));
        }
        return await query.OrderBy(p => p.Sku).ToListAsync(ct);
    }

    /// <summary>User input must not act as LIKE wildcards.</summary>
    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
             .Replace("%", "\\%", StringComparison.Ordinal)
             .Replace("_", "\\_", StringComparison.Ordinal)
             .Replace("[", "\\[", StringComparison.Ordinal);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.Sku == sku, ct);

    public void Add(Product product) => db.Products.Add(product);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
