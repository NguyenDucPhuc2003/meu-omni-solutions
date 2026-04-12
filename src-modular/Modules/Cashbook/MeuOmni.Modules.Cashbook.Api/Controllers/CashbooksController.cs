using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Cashbook.Application.Cashbooks;

namespace MeuOmni.Modules.Cashbook.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/cashbook/cashbooks")]
public sealed class CashbooksController(
    ICashbookApplicationService cashbookApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CashbookListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await cashbookApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCashbookRequest request, CancellationToken cancellationToken)
    {
        return Ok(await cashbookApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cashbook = await cashbookApplicationService.GetByIdAsync(id, cancellationToken);
        return cashbook is null ? NotFound() : Ok(cashbook);
    }

    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateCashbookRequest request, CancellationToken cancellationToken)
    {
        var cashbook = await cashbookApplicationService.UpdateAsync(id, request, cancellationToken);
        return cashbook is null ? NotFound() : Ok(cashbook);
    }

    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Read)]
    [HttpGet("{id:guid}/balance")]
    public async Task<IActionResult> GetBalance(Guid id, CancellationToken cancellationToken)
    {
        var balance = await cashbookApplicationService.GetBalanceAsync(id, cancellationToken);
        return balance is null ? NotFound() : Ok(balance);
    }

    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Read)]
    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(Guid id, [FromQuery] CashTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await cashbookApplicationService.GetTransactionsAsync(id, query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Cashbook.Cashbooks.Reconcile)]
    [HttpPost("{id:guid}/actions/reconcile")]
    public async Task<IActionResult> Reconcile(Guid id, [FromBody] ReconcileCashbookRequest request, CancellationToken cancellationToken)
    {
        var reconciliation = await cashbookApplicationService.ReconcileAsync(id, request, cancellationToken);
        return reconciliation is null ? NotFound() : Ok(reconciliation);
    }
}
