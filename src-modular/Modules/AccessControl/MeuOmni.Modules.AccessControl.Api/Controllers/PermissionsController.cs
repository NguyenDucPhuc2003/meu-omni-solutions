using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.AccessControl.Application.Scaffolding;

namespace MeuOmni.Modules.AccessControl.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/access-control/permissions")]
public sealed class PermissionsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.AccessControl.Permissions.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Permissions"),
            path = "/permissions",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
}
