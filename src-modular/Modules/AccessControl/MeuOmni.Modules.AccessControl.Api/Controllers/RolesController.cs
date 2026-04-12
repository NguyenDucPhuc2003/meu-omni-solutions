using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.AccessControl.Application.Scaffolding;

namespace MeuOmni.Modules.AccessControl.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/access-control/roles")]
public sealed class RolesController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.AccessControl.Roles.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Roles"),
            path = "/roles",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Roles.Create)]
    [HttpPost]
    public IActionResult Create([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Roles"),
            operation = "POST /roles",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Roles.Read)]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Roles"),
            path = "/roles/{id}",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Roles.Update)]
    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Roles"),
            operation = "PATCH /roles/{id}",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
}
