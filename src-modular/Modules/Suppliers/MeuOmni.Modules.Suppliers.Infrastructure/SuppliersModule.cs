using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Suppliers.Application.Suppliers;
using MeuOmni.Modules.Suppliers.Domain.Suppliers;
using MeuOmni.Modules.Suppliers.Infrastructure.Persistence;
using MeuOmni.Modules.Suppliers.Infrastructure.Repositories;

namespace MeuOmni.Modules.Suppliers.Infrastructure;

public sealed class SuppliersModule : IModuleDefinition
{
    public string ModuleName => "Suppliers";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Suppliers:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Suppliers database connection string is missing.");
        }

        services.AddDbContext<SuppliersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<ISupplierDebtTransactionRepository, SupplierDebtTransactionRepository>();
        services.AddScoped<ISupplierApplicationService, SupplierApplicationService>();
        services.AddScoped<ISupplierDebtTransactionApplicationService, SupplierDebtTransactionApplicationService>();
    }
}
