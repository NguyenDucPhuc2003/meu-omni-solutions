using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Operations.Application.Scaffolding;

namespace MeuOmni.Modules.Operations.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/operations/store-settings")]
public sealed class StoreSettingsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Operations.StoreSettings.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("StoreSettings"),
            path = "/store-settings",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.Operations.StoreSettings.Update)]
    [HttpPatch]
    public IActionResult Patch([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("StoreSettings"),
            operation = "PATCH /store-settings",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
}
