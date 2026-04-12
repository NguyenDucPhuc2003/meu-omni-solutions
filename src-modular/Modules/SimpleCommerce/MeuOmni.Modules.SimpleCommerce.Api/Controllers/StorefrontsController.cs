using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Commands;
using MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Services;

namespace MeuOmni.Modules.SimpleCommerce.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/simple-commerce/storefronts")]
public sealed class StorefrontsController(
    IStorefrontApplicationService storefrontApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SimpleCommerce.Storefronts.Read)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await storefrontApplicationService.ListAsync(cancellationToken));
    }

    [HttpGet("{storefrontId:guid}")]
    [RequirePermission(PermissionCodes.SimpleCommerce.Storefronts.Read)]
    public async Task<IActionResult> GetById(Guid storefrontId, CancellationToken cancellationToken)
    {
        var storefront = await storefrontApplicationService.GetByIdAsync(storefrontId, cancellationToken);
        return storefront is null ? NotFound() : Ok(storefront);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.SimpleCommerce.Storefronts.Create)]
    public async Task<IActionResult> Create([FromBody] CreateStorefrontRequest request, CancellationToken cancellationToken)
    {
        return Ok(await storefrontApplicationService.CreateAsync(request, cancellationToken));
    }
}
