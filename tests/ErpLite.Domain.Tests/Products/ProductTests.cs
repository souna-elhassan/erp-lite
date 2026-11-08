using ErpLite.Domain.Common;
using ErpLite.Domain.Products;

namespace ErpLite.Domain.Tests.Products;

public class ProductTests
{
    [Fact]
    public void Create_normalizes_sku_and_starts_active_with_no_stock()
    {
        var product = Product.Create("  glv-nit-m ", " Nitrile gloves ", 89m);

        Assert.Equal("GLV-NIT-M", product.Sku);
        Assert.Equal("Nitrile gloves", product.Name);
        Assert.Equal(0, product.QuantityOnHand);
        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Theory]
    [InlineData("", "Name", 1)]
    [InlineData("SKU", " ", 1)]
    [InlineData("SKU", "Name", -0.01)]
    public void Create_rejects_invalid_input(string sku, string name, double price)
    {
        Assert.Throws<DomainException>(() => Product.Create(sku, name, (decimal)price));
    }

    [Fact]
    public void Create_rejects_sku_longer_than_limit()
    {
        var sku = new string('A', Product.SkuMaxLength + 1);
        Assert.Throws<DomainException>(() => Product.Create(sku, "Name", 1m));
    }

    [Fact]
    public void ChangePrice_rounds_to_two_decimals()
    {
        var product = Product.Create("SKU", "Name", 10.005m);
        Assert.Equal(10.01m, product.UnitPrice);
    }

    [Fact]
    public void ReceiveStock_then_IssueStock_updates_quantity()
    {
        var product = Product.Create("SKU", "Name", 1m);

        product.ReceiveStock(10);
        product.IssueStock(4);

        Assert.Equal(6, product.QuantityOnHand);
    }

    [Fact]
    public void IssueStock_cannot_go_below_zero()
    {
        var product = Product.Create("SKU", "Name", 1m);
        product.ReceiveStock(3);

        var ex = Assert.Throws<DomainException>(() => product.IssueStock(4));

        Assert.Contains("Insufficient stock", ex.Message, StringComparison.Ordinal);
        Assert.Equal(3, product.QuantityOnHand);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Stock_movements_require_a_positive_quantity(int quantity)
    {
        var product = Product.Create("SKU", "Name", 1m);
        product.ReceiveStock(10);

        Assert.Throws<DomainException>(() => product.ReceiveStock(quantity));
        Assert.Throws<DomainException>(() => product.IssueStock(quantity));
    }

    [Fact]
    public void Inactive_product_cannot_issue_stock_but_can_receive()
    {
        var product = Product.Create("SKU", "Name", 1m);
        product.ReceiveStock(5);
        product.Deactivate();

        Assert.Throws<DomainException>(() => product.IssueStock(1));
        product.ReceiveStock(1);
        Assert.Equal(6, product.QuantityOnHand);
    }
}
