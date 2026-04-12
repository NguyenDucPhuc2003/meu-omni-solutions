using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Commands;
using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Dtos;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Entities;
using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Repositories;

namespace MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Services;

public sealed class StorefrontApplicationService(
    IStorefrontRepository storefrontRepository,
    ITenantContextAccessor tenantContextAccessor) : IStorefrontApplicationService
{
    public async Task<StorefrontDto> CreateAsync(CreateStorefrontRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var storefront = new Storefront(tenantId, request.Name, request.BaseUrl, request.LinkedSalesChannel);
        await storefrontRepository.AddAsync(storefront, cancellationToken);
        await storefrontRepository.SaveChangesAsync(cancellationToken);
        return ToDto(storefront);
    }

    public async Task<StorefrontDto?> GetByIdAsync(Guid storefrontId, CancellationToken cancellationToken = default)
    {
        var storefront = await storefrontRepository.GetByIdAsync(storefrontId, cancellationToken);
        return storefront is null ? null : ToDto(storefront);
    }

    public async Task<List<StorefrontDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var storefronts = await storefrontRepository.ListAsync(cancellationToken);
        return storefronts.Select(ToDto).ToList();
    }

    private static StorefrontDto ToDto(Storefront storefront)
    {
        return new StorefrontDto
        {
            Id = storefront.Id,
            TenantId = storefront.TenantId,
            Name = storefront.Name,
            BaseUrl = storefront.BaseUrl,
            LinkedSalesChannel = storefront.LinkedSalesChannel,
            IsActive = storefront.IsActive
        };
    }
}
