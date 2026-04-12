using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Customers.Application.Customers;
using MeuOmni.Modules.Customers.Domain.Customers;
using MeuOmni.Modules.Customers.Infrastructure.Persistence;
using MeuOmni.Modules.Customers.Infrastructure.Repositories;

namespace MeuOmni.Modules.Customers.Infrastructure;

public sealed class CustomersModule : IModuleDefinition
{
    public string ModuleName => "Customers";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Customers:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Customers database connection string is missing.");
        }

        services.AddDbContext<CustomersDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerDebtTransactionRepository, CustomerDebtTransactionRepository>();
        services.AddScoped<ICustomerApplicationService, CustomerApplicationService>();
        services.AddScoped<ICustomerDebtTransactionApplicationService, CustomerDebtTransactionApplicationService>();
    }
}
