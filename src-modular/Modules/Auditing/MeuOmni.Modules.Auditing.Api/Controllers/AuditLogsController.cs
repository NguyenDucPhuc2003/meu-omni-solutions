using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Auditing.Application.Scaffolding;

namespace MeuOmni.Modules.Auditing.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/auditing/audit-logs")]
public sealed class AuditLogsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Auditing.Logs.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Auditing",
            resource = AuditingScaffoldCatalog.GetResource("AuditLogs"),
            path = "/audit-logs",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.Auditing.Logs.Read)]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Auditing",
            resource = AuditingScaffoldCatalog.GetResource("AuditLogs"),
            path = "/audit-logs/{id}",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
}
