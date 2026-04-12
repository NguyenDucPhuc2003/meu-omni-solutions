using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.SalesChannel.Application.Orders.Services;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Repositories;
using MeuOmni.Modules.SalesChannel.Infrastructure.Persistence;
using MeuOmni.Modules.SalesChannel.Infrastructure.Repositories;

namespace MeuOmni.Modules.SalesChannel.Infrastructure;

public sealed class SalesChannelModule : IModuleDefinition
{
    public string ModuleName => "SalesChannel";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:SalesChannel:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("SalesChannel database connection string is missing.");
        }

        services.AddDbContext<SalesChannelDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<ISalesOrderApplicationService, SalesOrderApplicationService>();
    }
}
