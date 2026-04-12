using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeuOmni.BuildingBlocks.Modules;

public interface IModuleDefinition
{
    string ModuleName { get; }

    void AddModule(IServiceCollection services, IConfiguration configuration);
}
