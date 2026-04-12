using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Catalog.Application.Catalog;
using MeuOmni.Modules.Catalog.Domain.Catalog;
using MeuOmni.Modules.Catalog.Infrastructure.Persistence;
using MeuOmni.Modules.Catalog.Infrastructure.Repositories;

namespace MeuOmni.Modules.Catalog.Infrastructure;

public sealed class CatalogModule : IModuleDefinition
{
    public string ModuleName => "Catalog";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Catalog:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Catalog database connection string is missing.");
        }

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductPriceRepository, ProductPriceRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductApplicationService, ProductApplicationService>();
        services.AddScoped<IProductPriceApplicationService, ProductPriceApplicationService>();
        services.AddScoped<ICategoryApplicationService, CategoryApplicationService>();
    }
}
