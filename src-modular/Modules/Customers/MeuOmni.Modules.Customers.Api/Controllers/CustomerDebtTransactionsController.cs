using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Customers.Application.Customers;

namespace MeuOmni.Modules.Customers.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/customers/customer-debt-transactions")]
public sealed class CustomerDebtTransactionsController(
    ICustomerDebtTransactionApplicationService customerDebtTransactionApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Customers.DebtTransactions.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CustomerDebtTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await customerDebtTransactionApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Customers.DebtTransactions.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDebtTransactionRequest request, CancellationToken cancellationToken)
    {
        var transaction = await customerDebtTransactionApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, transaction);
    }

    [RequirePermission(PermissionCodes.Customers.DebtTransactions.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var transaction = await customerDebtTransactionApplicationService.GetByIdAsync(id, cancellationToken);
        return transaction is null ? NotFound() : Ok(transaction);
    }
}
