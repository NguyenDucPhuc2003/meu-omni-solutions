using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Entities;
using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Repositories;
using MeuOmni.Modules.SimpleCommerce.Infrastructure.Persistence;

namespace MeuOmni.Modules.SimpleCommerce.Infrastructure.Repositories;

public sealed class StorefrontRepository(SimpleCommerceDbContext dbContext) : IStorefrontRepository
{
    public async Task AddAsync(Storefront storefront, CancellationToken cancellationToken = default)
    {
        await dbContext.Storefronts.AddAsync(storefront, cancellationToken);
    }

    public Task<Storefront?> GetByIdAsync(Guid storefrontId, CancellationToken cancellationToken = default)
    {
        return dbContext.Storefronts.FirstOrDefaultAsync(x => x.Id == storefrontId, cancellationToken);
    }

    public Task<List<Storefront>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Storefronts
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
