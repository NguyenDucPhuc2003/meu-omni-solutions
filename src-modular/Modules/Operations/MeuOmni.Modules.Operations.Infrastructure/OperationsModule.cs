using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Operations.Domain.Scaffold.Repositories;
using MeuOmni.Modules.Operations.Infrastructure.Persistence;
using MeuOmni.Modules.Operations.Infrastructure.Repositories;

namespace MeuOmni.Modules.Operations.Infrastructure;

public sealed class OperationsModule : IModuleDefinition
{
    public string ModuleName => "Operations";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Operations:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Operations database connection string is missing.");
        }

        services.AddDbContext<OperationsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IPrinterRepository, PrinterRepository>();
        services.AddScoped<IStoreSettingRepository, StoreSettingRepository>();
        services.AddScoped<IOperationalJobRepository, OperationalJobRepository>();
    }
}
