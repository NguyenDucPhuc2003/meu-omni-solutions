using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Suppliers.Application.Suppliers;

namespace MeuOmni.Modules.Suppliers.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/suppliers/supplier-debt-transactions")]
public sealed class SupplierDebtTransactionsController(
    ISupplierDebtTransactionApplicationService supplierDebtTransactionApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Suppliers.DebtTransactions.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SupplierDebtTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await supplierDebtTransactionApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Suppliers.DebtTransactions.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDebtTransactionRequest request, CancellationToken cancellationToken)
    {
        return Ok(await supplierDebtTransactionApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Suppliers.DebtTransactions.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await supplierDebtTransactionApplicationService.GetByIdAsync(id, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }
}
