using ErpLite.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ErpLite.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", t =>
        {
            // Defense in depth: the domain enforces these rules, the database does too.
            t.HasCheckConstraint("CK_Products_UnitPrice", "[UnitPrice] >= 0");
            t.HasCheckConstraint("CK_Products_QuantityOnHand", "[QuantityOnHand] >= 0");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Sku).HasMaxLength(Product.SkuMaxLength).IsRequired();
        builder.HasIndex(p => p.Sku).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(p => p.UnitPrice).HasPrecision(18, 2);
        builder.Property(p => p.RowVersion).IsRowVersion();
    }
}
