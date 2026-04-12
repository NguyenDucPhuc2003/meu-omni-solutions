using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace MeuOmni.BuildingBlocks.Querying;

public static class MeuOmniQueryingExtensions
{
    public static IServiceCollection AddMeuOmniQuerying(this IServiceCollection services)
    {
        services.AddScoped<ISieveProcessor, SieveProcessor>();
        return services;
    }
}
