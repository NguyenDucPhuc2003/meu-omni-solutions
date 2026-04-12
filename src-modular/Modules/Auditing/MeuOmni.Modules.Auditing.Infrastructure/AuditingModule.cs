using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.Auditing.Domain.Scaffold.Repositories;
using MeuOmni.Modules.Auditing.Infrastructure.Persistence;
using MeuOmni.Modules.Auditing.Infrastructure.Repositories;

namespace MeuOmni.Modules.Auditing.Infrastructure;

public sealed class AuditingModule : IModuleDefinition
{
    public string ModuleName => "Auditing";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:Auditing:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Auditing database connection string is missing.");
        }

        services.AddDbContext<AuditingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    }
}
