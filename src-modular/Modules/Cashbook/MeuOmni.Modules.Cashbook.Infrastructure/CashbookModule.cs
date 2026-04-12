using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Cashbook.Application.Cashbooks;
using MeuOmni.Modules.Cashbook.Domain.Cashbooks;
using MeuOmni.Modules.Cashbook.Infrastructure.Persistence;
using MeuOmni.Modules.Cashbook.Infrastructure.Repositories;

namespace MeuOmni.Modules.Cashbook.Infrastructure;

public sealed class CashbookModule : IModuleDefinition
{
    public string ModuleName => "Cashbook";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Cashbook:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Cashbook database connection string is missing.");
        }

        services.AddDbContext<CashbookDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ICashbookRepository, CashbookRepository>();
        services.AddScoped<ICashTransactionRepository, CashTransactionRepository>();
        services.AddScoped<ICashReconciliationRepository, CashReconciliationRepository>();
        services.AddScoped<ICashbookApplicationService, CashbookApplicationService>();
        services.AddScoped<ICashTransactionApplicationService, CashTransactionApplicationService>();
    }
}
