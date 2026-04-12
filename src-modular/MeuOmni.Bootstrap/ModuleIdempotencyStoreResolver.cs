using Microsoft.AspNetCore.Http;
using MeuOmni.BuildingBlocks.Idempotency;
using MeuOmni.Modules.Cashbook.Infrastructure.Persistence;
using MeuOmni.Modules.Inventory.Infrastructure.Persistence;
using MeuOmni.Modules.SalesChannel.Infrastructure.Persistence;

namespace MeuOmni.Bootstrap;

public sealed class ModuleIdempotencyStoreResolver(IServiceProvider serviceProvider) : IIdempotencyStoreResolver
{
    public IIdempotencyStore? Resolve(HttpContext context)
    {
        var path = context.Request.Path;

        if (path.StartsWithSegments("/api/modules/sales-channel", StringComparison.OrdinalIgnoreCase))
        {
            return new EntityFrameworkIdempotencyStore<SalesChannelDbContext>(
                serviceProvider.GetRequiredService<SalesChannelDbContext>());
        }

        if (path.StartsWithSegments("/api/modules/cashbook", StringComparison.OrdinalIgnoreCase))
        {
            return new EntityFrameworkIdempotencyStore<CashbookDbContext>(
                serviceProvider.GetRequiredService<CashbookDbContext>());
        }

        if (path.StartsWithSegments("/api/modules/inventory", StringComparison.OrdinalIgnoreCase))
        {
            return new EntityFrameworkIdempotencyStore<InventoryDbContext>(
                serviceProvider.GetRequiredService<InventoryDbContext>());
        }

        return null;
    }
}
