using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Operations.Application.Scaffolding;

namespace MeuOmni.Modules.Operations.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/operations/operations")]
public sealed class OperationsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Operations.Control.Backup)]
    [HttpPost("actions/backup")]
    public IActionResult ActionsBackup([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Operations"),
            operation = "POST /operations/actions/backup",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.Operations.Control.Export)]
    [HttpPost("actions/export")]
    public IActionResult ActionsExport([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Operations"),
            operation = "POST /operations/actions/export",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
}
