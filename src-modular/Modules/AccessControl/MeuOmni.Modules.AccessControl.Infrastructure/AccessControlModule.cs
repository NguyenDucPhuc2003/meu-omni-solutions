using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MeuOmni.BuildingBlocks.Modules;
using MeuOmni.Modules.AccessControl.Domain.Scaffold.Repositories;
using MeuOmni.Modules.AccessControl.Infrastructure.Persistence;
using MeuOmni.Modules.AccessControl.Infrastructure.Repositories;

namespace MeuOmni.Modules.AccessControl.Infrastructure;

public sealed class AccessControlModule : IModuleDefinition
{
    public string ModuleName => "AccessControl";

    public void AddModule(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Modules:AccessControl:Database:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("AccessControl database connection string is missing.");
        }

        services.AddDbContext<AccessControlDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<ILoginSessionRepository, LoginSessionRepository>();
    }
}
