using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Inventory.Application.Inventory;

namespace MeuOmni.Modules.Inventory.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/inventory/stock-levels")]
public sealed class StockLevelsController(
    IStockLevelApplicationService stockLevelApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Inventory.StockLevels.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StockLevelListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await stockLevelApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Inventory.StockLevels.Read)]
    [HttpGet("{warehouseId:guid}/{productId:guid}")]
    public async Task<IActionResult> GetById(Guid warehouseId, Guid productId, CancellationToken cancellationToken)
    {
        var stockLevel = await stockLevelApplicationService.GetByWarehouseProductAsync(warehouseId, productId, cancellationToken);
        return stockLevel is null ? NotFound() : Ok(stockLevel);
    }
}
