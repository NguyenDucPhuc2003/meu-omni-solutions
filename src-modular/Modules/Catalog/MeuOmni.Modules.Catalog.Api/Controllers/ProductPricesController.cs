using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Catalog.Application.Catalog;

namespace MeuOmni.Modules.Catalog.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/catalog/product-prices")]
public sealed class ProductPricesController(
    IProductPriceApplicationService productPriceApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Catalog.Prices.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateProductPriceRequest request, CancellationToken cancellationToken)
    {
        var price = await productPriceApplicationService.UpdateAsync(id, request, cancellationToken);
        return price is null ? NotFound() : Ok(price);
    }
}
