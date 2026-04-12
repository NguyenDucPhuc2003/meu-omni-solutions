using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Idempotency;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Inventory.Application.Inventory;

namespace MeuOmni.Modules.Inventory.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/inventory/stock-transactions")]
public sealed class StockTransactionsController(
    IStockTransactionApplicationService stockTransactionApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Inventory.StockTransactions.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] StockTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await stockTransactionApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Inventory.StockTransactions.Create)]
    [RequireIdempotency]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockTransactionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await stockTransactionApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Inventory.StockTransactions.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await stockTransactionApplicationService.GetByIdAsync(id, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [RequirePermission(PermissionCodes.Inventory.StockTransactions.Cancel)]
    [HttpPost("{id:guid}/actions/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelStockTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await stockTransactionApplicationService.CancelAsync(id, request, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }
}
