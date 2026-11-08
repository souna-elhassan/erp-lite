using ErpLite.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace ErpLite.Infrastructure.Persistence;

/// <summary>Fake demo data, only used in Development. No real company data.</summary>
public static class DevelopmentSeeder
{
    public static async Task SeedAsync(ErpLiteDbContext db, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (await db.Products.AnyAsync(ct)) return;

        var items = new (string Sku, string Name, decimal Price, int Qty)[]
        {
            ("GLV-NIT-M", "Nitrile gloves, size M (box of 100)", 89.00m, 240),
            ("MSK-FFP2", "FFP2 face mask (box of 20)", 120.00m, 75),
            ("THR-IR-01", "Infrared thermometer", 349.90m, 18),
            ("BP-ARM-02", "Arm blood pressure monitor", 590.00m, 9),
            ("OXI-FNG-01", "Finger pulse oximeter", 215.50m, 0),
        };

        foreach (var (sku, name, price, qty) in items)
        {
            var product = Product.Create(sku, name, price);
            if (qty > 0) product.ReceiveStock(qty);
            db.Products.Add(product);
        }
        await db.SaveChangesAsync(ct);
    }
}
