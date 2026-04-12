using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Catalog.Application.Catalog;

namespace MeuOmni.Modules.Catalog.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/catalog/products")]
public sealed class ProductsController(
    IProductApplicationService productApplicationService,
    IProductPriceApplicationService productPriceApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Catalog.Products.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await productApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Catalog.Products.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        return Ok(await productApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Catalog.Products.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productApplicationService.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [RequirePermission(PermissionCodes.Catalog.Products.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await productApplicationService.UpdateAsync(id, request, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [RequirePermission(PermissionCodes.Catalog.Prices.Read)]
    [HttpGet("{id:guid}/prices")]
    public async Task<IActionResult> GetPrices(Guid id, [FromQuery] ProductPriceListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await productPriceApplicationService.ListAsync(id, query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Catalog.Prices.Create)]
    [HttpPost("{id:guid}/prices")]
    public async Task<IActionResult> Prices(Guid id, [FromBody] SetProductPriceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await productPriceApplicationService.CreateAsync(id, request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Catalog.Products.Activate)]
    [HttpPost("{id:guid}/actions/activate")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ProductStatusActionRequest request, CancellationToken cancellationToken)
    {
        var product = await productApplicationService.ActivateAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [RequirePermission(PermissionCodes.Catalog.Products.Deactivate)]
    [HttpPost("{id:guid}/actions/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] ProductStatusActionRequest request, CancellationToken cancellationToken)
    {
        var product = await productApplicationService.DeactivateAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [RequirePermission(PermissionCodes.Catalog.Products.SetPrice)]
    [HttpPost("{id:guid}/actions/set-price")]
    public async Task<IActionResult> SetPrice(Guid id, [FromBody] SetProductPriceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await productPriceApplicationService.CreateAsync(id, request, cancellationToken));
    }
}
