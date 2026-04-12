using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Cashbook.Application.Cashbooks;

namespace MeuOmni.Modules.Cashbook.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/cashbook/cash-transactions")]
public sealed class CashTransactionsController(
    ICashTransactionApplicationService cashTransactionApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Cashbook.CashTransactions.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CashTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await cashTransactionApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Cashbook.CashTransactions.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCashTransactionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await cashTransactionApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Cashbook.CashTransactions.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await cashTransactionApplicationService.GetByIdAsync(id, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [RequirePermission(PermissionCodes.Cashbook.CashTransactions.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateCashTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await cashTransactionApplicationService.UpdateAsync(id, request, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }

    [RequirePermission(PermissionCodes.Cashbook.CashTransactions.Cancel)]
    [HttpPost("{id:guid}/actions/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelCashTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await cashTransactionApplicationService.CancelAsync(id, request, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }
}
