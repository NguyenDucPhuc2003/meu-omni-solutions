using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Reporting.Application.Scaffolding;

namespace MeuOmni.Modules.Reporting.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/reporting/reports/inventory")]
public sealed class InventoryReportsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Reports.Inventory.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Reporting",
            resource = ReportingScaffoldCatalog.GetResource("Inventory"),
            path = "/reports/inventory",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
}
