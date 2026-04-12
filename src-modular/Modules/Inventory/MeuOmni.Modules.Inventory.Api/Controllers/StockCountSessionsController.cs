using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Inventory.Application.Inventory;

namespace MeuOmni.Modules.Inventory.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/inventory/stock-count-sessions")]
public sealed class StockCountSessionsController(
    IStockCountSessionApplicationService stockCountSessionApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Inventory.StockCountSessions.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StockCountSessionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await stockCountSessionApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Inventory.StockCountSessions.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockCountSessionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await stockCountSessionApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Inventory.StockCountSessions.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var session = await stockCountSessionApplicationService.GetByIdAsync(id, cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }

    [RequirePermission(PermissionCodes.Inventory.StockCountSessions.Complete)]
    [HttpPost("{id:guid}/actions/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteStockCountSessionRequest request, CancellationToken cancellationToken)
    {
        var session = await stockCountSessionApplicationService.CompleteAsync(id, request, cancellationToken);
        return session is null ? NotFound() : Ok(session);
    }
}
