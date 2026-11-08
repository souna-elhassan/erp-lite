using ErpLite.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace ErpLite.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        return services;
    }
}
