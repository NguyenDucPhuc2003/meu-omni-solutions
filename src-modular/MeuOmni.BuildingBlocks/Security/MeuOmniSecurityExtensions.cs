using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MeuOmni.BuildingBlocks.Security;

public static class MeuOmniSecurityExtensions
{
    public static IServiceCollection AddMeuOmniRequestSecurity(this IServiceCollection services)
    {
        services.AddScoped<TenantContextAccessor>();
        services.AddScoped<ITenantContextAccessor>(sp => sp.GetRequiredService<TenantContextAccessor>());

        services.AddScoped<CurrentUserContextAccessor>();
        services.AddScoped<ICurrentUserContextAccessor>(sp => sp.GetRequiredService<CurrentUserContextAccessor>());

        return services;
    }

    public static IApplicationBuilder UseMeuOmniRequestSecurity(this IApplicationBuilder app)
    {
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseMiddleware<CurrentUserContextMiddleware>();
        app.UseMiddleware<EndpointAuthorizationMiddleware>();

        return app;
    }
}
