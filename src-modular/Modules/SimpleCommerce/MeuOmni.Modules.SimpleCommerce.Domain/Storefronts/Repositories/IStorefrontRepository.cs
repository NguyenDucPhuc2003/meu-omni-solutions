using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Entities;

namespace MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Repositories;

public interface IStorefrontRepository
{
    Task AddAsync(Storefront storefront, CancellationToken cancellationToken = default);

    Task<Storefront?> GetByIdAsync(Guid storefrontId, CancellationToken cancellationToken = default);

    Task<List<Storefront>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
