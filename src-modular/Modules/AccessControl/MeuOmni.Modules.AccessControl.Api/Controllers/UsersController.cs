using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.AccessControl.Application.Scaffolding;

namespace MeuOmni.Modules.AccessControl.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/access-control/users")]
public sealed class UsersController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.AccessControl.Users.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            path = "/users",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Users.Create)]
    [HttpPost]
    public IActionResult Create([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            operation = "POST /users",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Users.Read)]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            path = "/users/{id}",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Users.Update)]
    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            operation = "PATCH /users/{id}",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Users.Activate)]
    [HttpPost("{id:guid}/actions/activate")]
    public IActionResult Activate(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            operation = "POST /users/{id}/actions/activate",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Users.Deactivate)]
    [HttpPost("{id:guid}/actions/deactivate")]
    public IActionResult Deactivate(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            operation = "POST /users/{id}/actions/deactivate",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.AccessControl.Users.ResetPassword)]
    [HttpPost("{id:guid}/actions/reset-password")]
    public IActionResult ResetPassword(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Users"),
            operation = "POST /users/{id}/actions/reset-password",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
}
