using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Inventory.Application.Inventory;
using MeuOmni.Modules.Inventory.Domain.Inventory;
using MeuOmni.Modules.Inventory.Infrastructure.Persistence;
using MeuOmni.Modules.Inventory.Infrastructure.Repositories;

namespace MeuOmni.Modules.Inventory.Infrastructure;

public sealed class InventoryModule : IModuleDefinition
{
    public string ModuleName => "Inventory";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Inventory:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Inventory database connection string is missing.");
        }

        services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IStockTransactionRepository, StockTransactionRepository>();
        services.AddScoped<IStockLevelRepository, StockLevelRepository>();
        services.AddScoped<IStockCountSessionRepository, StockCountSessionRepository>();
        services.AddScoped<IStockTransactionApplicationService, StockTransactionApplicationService>();
        services.AddScoped<IStockLevelApplicationService, StockLevelApplicationService>();
        services.AddScoped<IStockCountSessionApplicationService, StockCountSessionApplicationService>();
    }
}
