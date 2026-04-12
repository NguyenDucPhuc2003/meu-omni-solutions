using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Commands;
using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Dtos;

namespace MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Services;

public interface IStorefrontApplicationService
{
    Task<StorefrontDto> CreateAsync(CreateStorefrontRequest request, CancellationToken cancellationToken = default);

    Task<StorefrontDto?> GetByIdAsync(Guid storefrontId, CancellationToken cancellationToken = default);

    Task<List<StorefrontDto>> ListAsync(CancellationToken cancellationToken = default);
}
