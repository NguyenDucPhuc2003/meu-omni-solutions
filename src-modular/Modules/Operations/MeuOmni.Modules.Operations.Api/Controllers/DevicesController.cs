using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Operations.Application.Scaffolding;

namespace MeuOmni.Modules.Operations.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/operations/devices")]
public sealed class DevicesController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Operations.Devices.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Devices"),
            path = "/devices",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.Operations.Devices.Create)]
    [HttpPost]
    public IActionResult Create([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Devices"),
            operation = "POST /devices",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.Operations.Devices.Read)]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Devices"),
            path = "/devices/{id}",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.Operations.Devices.Update)]
    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Devices"),
            operation = "PATCH /devices/{id}",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.Operations.Devices.Test)]
    [HttpPost("{id:guid}/actions/test")]
    public IActionResult Test(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Devices"),
            operation = "POST /devices/{id}/actions/test",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
}
