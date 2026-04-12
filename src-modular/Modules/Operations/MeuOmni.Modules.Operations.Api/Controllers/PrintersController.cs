using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.Operations.Application.Scaffolding;

namespace MeuOmni.Modules.Operations.Api.Controllers;

[RequireRole("Admin")]
[Route("api/modules/operations/printers")]
public sealed class PrintersController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [RequirePermission(PermissionCodes.Operations.Printers.Read)]
    [HttpGet]
    public IActionResult GetAll(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Printers"),
            path = "/printers",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.Operations.Printers.Create)]
    [HttpPost]
    public IActionResult Create([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Printers"),
            operation = "POST /printers",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.Operations.Printers.Read)]
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Printers"),
            path = "/printers/{id}",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
    [RequirePermission(PermissionCodes.Operations.Printers.Update)]
    [HttpPatch("{id:guid}")]
    public IActionResult Patch(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Printers"),
            operation = "PATCH /printers/{id}",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
    [RequirePermission(PermissionCodes.Operations.Printers.TestPrint)]
    [HttpPost("{id:guid}/actions/test-print")]
    public IActionResult TestPrint(Guid id, [FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "Operations",
            resource = OperationsScaffoldCatalog.GetResource("Printers"),
            operation = "POST /printers/{id}/actions/test-print",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }
}
