using ErpLite.Domain.Common;

namespace ErpLite.Domain.Products;

/// <summary>
/// A sellable item tracked in stock. All changes go through methods that enforce the business rules,
/// so a product can never be in an invalid state (empty SKU, negative price, negative stock).
/// </summary>
public sealed class Product
{
    public const int SkuMaxLength = 32;
    public const int NameMaxLength = 200;

    public Guid Id { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int QuantityOnHand { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>Optimistic concurrency token (SQL Server rowversion).</summary>
    public byte[] RowVersion { get; private set; } = [];

    private Product() { } // EF Core

    public static Product Create(string sku, string name, decimal unitPrice)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Sku = NormalizeSku(sku),
            IsActive = true,
        };
        product.Rename(name);
        product.ChangePrice(unitPrice);
        return product;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        name = name.Trim();
        if (name.Length > NameMaxLength)
            throw new DomainException($"Product name cannot exceed {NameMaxLength} characters.");
        Name = name;
    }

    public void ChangePrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");
        UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero);
    }

    public void ReceiveStock(int quantity)
    {
        EnsurePositive(quantity);
        QuantityOnHand = checked(QuantityOnHand + quantity);
    }

    public void IssueStock(int quantity)
    {
        EnsurePositive(quantity);
        if (!IsActive)
            throw new DomainException("Cannot issue stock for an inactive product.");
        if (quantity > QuantityOnHand)
            throw new DomainException($"Insufficient stock: {QuantityOnHand} on hand, {quantity} requested.");
        QuantityOnHand -= quantity;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private static string NormalizeSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("SKU is required.");
        sku = sku.Trim().ToUpperInvariant();
        if (sku.Length > SkuMaxLength)
            throw new DomainException($"SKU cannot exceed {SkuMaxLength} characters.");
        return sku;
    }

    private static void EnsurePositive(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");
    }
}
