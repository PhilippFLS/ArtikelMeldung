using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FLS.CodeBeispiel.KatalogService.Infrastructure;

namespace FLS.CodeBeispiel.KatalogService;

public static class KatalogServiceExtension
{
    public static IServiceCollection AddKatalogService(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<KatalogContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddTransient<IKatalogService, Infrastructure.KatalogService>();

        return services;
    }
}