using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Reporting.Application.Scaffolding;

namespace MeuOmni.Modules.Reporting.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/reporting/reports/sales")]
public sealed class SalesReportsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Reports.Sales.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Reporting",
            resource = ReportingScaffoldCatalog.GetResource("Sales"),
            path = "/reports/sales",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
}
