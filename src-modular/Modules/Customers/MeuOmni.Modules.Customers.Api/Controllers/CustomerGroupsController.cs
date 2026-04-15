using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Customers.Application.Customers;

namespace MeuOmni.Modules.Customers.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/customers/customer-groups")]
public sealed class CustomerGroupsController(
    ICustomerGroupApplicationService customerGroupApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Customers.Groups.Read)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CustomerGroupListQuery query, CancellationToken cancellationToken)
    {
        return Ok(await customerGroupApplicationService.ListAsync(query, cancellationToken));
    }

    [RequirePermission(PermissionCodes.Customers.Groups.Create)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await customerGroupApplicationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, group);
    }

    [RequirePermission(PermissionCodes.Customers.Groups.Read)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var group = await customerGroupApplicationService.GetByIdAsync(id, cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    [RequirePermission(PermissionCodes.Customers.Groups.Update)]
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] UpdateCustomerGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await customerGroupApplicationService.UpdateAsync(id, request, cancellationToken);
        return group is null ? NotFound() : Ok(group);
    }

    [RequirePermission(PermissionCodes.Customers.Groups.Delete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await customerGroupApplicationService.DeleteAsync(id, cancellationToken);
        return deleted ? Ok() : NotFound();
    }
}
