using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Reporting.Domain.Scaffold.Repositories;
using MeuOmni.Modules.Reporting.Infrastructure.Persistence;
using MeuOmni.Modules.Reporting.Infrastructure.Repositories;

namespace MeuOmni.Modules.Reporting.Infrastructure;

public sealed class ReportingModule : IModuleDefinition
{
    public string ModuleName => "Reporting";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Reporting:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Reporting database connection string is missing.");
        }

        services.AddDbContext<ReportingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ISalesDashboardReadModelRepository, SalesDashboardReadModelRepository>();
        services.AddScoped<IShiftSummaryReadModelRepository, ShiftSummaryReadModelRepository>();
        services.AddScoped<ISalesReportReadModelRepository, SalesReportReadModelRepository>();
        services.AddScoped<IInventorySummaryReadModelRepository, InventorySummaryReadModelRepository>();
        services.AddScoped<ICashFlowReadModelRepository, CashFlowReadModelRepository>();
        services.AddScoped<ICustomerDebtReportReadModelRepository, CustomerDebtReportReadModelRepository>();
        services.AddScoped<ISupplierDebtReportReadModelRepository, SupplierDebtReportReadModelRepository>();
    }
}
