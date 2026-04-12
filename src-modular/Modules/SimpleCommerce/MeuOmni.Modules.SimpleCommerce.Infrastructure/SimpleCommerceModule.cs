using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Services;
using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Repositories;
using MeuOmni.Modules.SimpleCommerce.Infrastructure.Persistence;
using MeuOmni.Modules.SimpleCommerce.Infrastructure.Repositories;

namespace MeuOmni.Modules.SimpleCommerce.Infrastructure;

public sealed class SimpleCommerceModule : IModuleDefinition
{
    public string ModuleName => "SimpleCommerce";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:SimpleCommerce:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("SimpleCommerce database connection string is missing.");
        }

        services.AddDbContext<SimpleCommerceDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IStorefrontRepository, StorefrontRepository>();
        services.AddScoped<IStorefrontApplicationService, StorefrontApplicationService>();
    }
}
