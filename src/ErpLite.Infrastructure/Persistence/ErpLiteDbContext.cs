using ErpLite.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace ErpLite.Infrastructure.Persistence;

public sealed class ErpLiteDbContext(DbContextOptions<ErpLiteDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpLiteDbContext).Assembly);
    }
}
