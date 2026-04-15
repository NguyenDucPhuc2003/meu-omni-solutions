using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Customers.Application.Customers;

namespace MeuOmni.Modules.Customers.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/customers/customers")]
public sealed class CustomersController(
    ICustomerApplicationService customerApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Customers.Profiles.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CustomerListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await customerApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customerApplicationService.GetByIdAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerApplicationService.UpdateAsync(id, request, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Read)]
    [HttpGet("{id:guid}/purchase-history")]
    public async Task<IActionResult> GetPurchaseHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await customerApplicationService.GetPurchaseHistoryAsync(id, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Read)]
    [HttpGet("{id:guid}/statistics")]
    public async Task<IActionResult> GetStatistics(Guid id, CancellationToken cancellationToken)
    {
        var statistics = await customerApplicationService.GetStatisticsAsync(id, cancellationToken);
        return statistics is null ? NotFound() : Ok(statistics);
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Read)]
    [HttpGet("{id:guid}/debt-summary")]
    public async Task<IActionResult> GetDebtSummary(Guid id, CancellationToken cancellationToken)
    {
        var summary = await customerApplicationService.GetDebtSummaryAsync(id, cancellationToken);
        return summary is null ? NotFound() : Ok(summary);
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Read)]
    [HttpGet("{id:guid}/debt-transactions")]
    public async Task<IActionResult> GetDebtTransactions(Guid id, [FromQuery] CustomerDebtTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await customerApplicationService.GetDebtTransactionsAsync(id, query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Activate)]
    [HttpPost("{id:guid}/actions/activate")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] CustomerStatusActionRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerApplicationService.ActivateAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [RequirePermission(PermissionCodes.Customers.Profiles.Deactivate)]
    [HttpPost("{id:guid}/actions/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] CustomerStatusActionRequest request, CancellationToken cancellationToken)
    {
        var customer = await customerApplicationService.DeactivateAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }
}
