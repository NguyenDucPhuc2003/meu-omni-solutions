using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Suppliers.Application.Suppliers;

namespace MeuOmni.Modules.Suppliers.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/suppliers/suppliers")]
public sealed class SuppliersController(
    ISupplierApplicationService supplierApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Suppliers.Profiles.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SupplierListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await supplierApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        return Ok(await supplierApplicationService.CreateAsync(request, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var supplier = await supplierApplicationService.GetByIdAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var supplier = await supplierApplicationService.UpdateAsync(id, request, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Read)]
    [HttpGet("{id:guid}/debt-summary")]
    public async Task<IActionResult> GetDebtSummary(Guid id, CancellationToken cancellationToken)
    {
        var summary = await supplierApplicationService.GetDebtSummaryAsync(id, cancellationToken);
        return summary is null ? NotFound() : Ok(summary);
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Read)]
    [HttpGet("{id:guid}/debt-transactions")]
    public async Task<IActionResult> GetDebtTransactions(Guid id, [FromQuery] SupplierDebtTransactionListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await supplierApplicationService.GetDebtTransactionsAsync(id, query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Activate)]
    [HttpPost("{id:guid}/actions/activate")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] SupplierStatusActionRequest request, CancellationToken cancellationToken)
    {
        var supplier = await supplierApplicationService.ActivateAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }

    [RequirePermission(PermissionCodes.Suppliers.Profiles.Deactivate)]
    [HttpPost("{id:guid}/actions/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] SupplierStatusActionRequest request, CancellationToken cancellationToken)
    {
        var supplier = await supplierApplicationService.DeactivateAsync(id, cancellationToken);
        return supplier is null ? NotFound() : Ok(supplier);
    }
}
