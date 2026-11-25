using Microsoft.Extensions.DependencyInjection;
using ProductCrud.Application.Products.Interfaces;
using ProductCrud.Application.Products.Services;

namespace ProductCrud.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
