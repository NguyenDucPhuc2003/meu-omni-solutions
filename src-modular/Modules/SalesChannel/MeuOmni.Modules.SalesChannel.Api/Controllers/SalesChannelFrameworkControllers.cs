using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Idempotency;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;

namespace MeuOmni.Modules.SalesChannel.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/sales-channel/shifts")]
public sealed class ShiftsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Read)]
    public IActionResult GetAll() => Ok(new { scaffold = true, resource = "shifts", tenantId = TenantId, userId = CurrentUserId });

    [HttpPost]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Create)]
    public IActionResult Create([FromBody] object? request) => Accepted(new { scaffold = true, resource = "shifts", operation = "create", tenantId = TenantId, request });

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Read)]
    public IActionResult GetById(Guid id) => Ok(new { scaffold = true, resource = "shifts", id, tenantId = TenantId });

    [HttpPatch("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Update)]
    public IActionResult Patch(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "shifts", id, operation = "patch", request, tenantId = TenantId });

    [HttpGet("current")]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Read)]
    public IActionResult GetCurrent() => Ok(new { scaffold = true, resource = "shifts", state = "current", tenantId = TenantId, userId = CurrentUserId });

    [HttpPost("{id:guid}/actions/close")]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Close)]
    public IActionResult Close(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "shifts", id, action = "close", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/reopen")]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Reopen)]
    public IActionResult Reopen(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "shifts", id, action = "reopen", request, tenantId = TenantId });

    [HttpGet("{id:guid}/summary")]
    [RequirePermission(PermissionCodes.SalesChannel.Shifts.Read)]
    public IActionResult GetSummary(Guid id) => Ok(new { scaffold = true, resource = "shifts", id, view = "summary", tenantId = TenantId });
}

[RequireRole("Admin", "Cashier")]
[Route("api/modules/sales-channel/bills")]
public sealed class BillsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Read)]
    public IActionResult GetAll() => Ok(new { scaffold = true, resource = "bills", tenantId = TenantId });

    [HttpPost]
    [RequireIdempotency]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Create)]
    public IActionResult Create([FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", operation = "create", request, tenantId = TenantId });

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Read)]
    public IActionResult GetById(Guid id) => Ok(new { scaffold = true, resource = "bills", id, tenantId = TenantId });

    [HttpPatch("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Update)]
    public IActionResult Patch(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, operation = "patch", request, tenantId = TenantId });

    [HttpDelete("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Delete)]
    public IActionResult Delete(Guid id) => Accepted(new { scaffold = true, resource = "bills", id, operation = "delete", tenantId = TenantId });

    [HttpPost("{id:guid}/actions/add-item")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.AddItem)]
    public IActionResult AddItem(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "add-item", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/update-item")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.UpdateItem)]
    public IActionResult UpdateItem(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "update-item", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/remove-item")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.RemoveItem)]
    public IActionResult RemoveItem(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "remove-item", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/attach-customer")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.AttachCustomer)]
    public IActionResult AttachCustomer(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "attach-customer", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/apply-adjustment")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.ApplyAdjustment)]
    public IActionResult ApplyAdjustment(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "apply-adjustment", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/hold")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Hold)]
    public IActionResult Hold(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "hold", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/resume")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Resume)]
    public IActionResult Resume(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "resume", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/complete")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Complete)]
    public IActionResult Complete(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "complete", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/cancel")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Cancel)]
    public IActionResult Cancel(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "cancel", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/reprint")]
    [RequirePermission(PermissionCodes.SalesChannel.Bills.Reprint)]
    public IActionResult Reprint(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "bills", id, action = "reprint", request, tenantId = TenantId });
}

[RequireRole("Admin", "Cashier")]
[Route("api/modules/sales-channel/payments")]
public sealed class PaymentsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SalesChannel.Payments.Read)]
    public IActionResult GetAll() => Ok(new { scaffold = true, resource = "payments", tenantId = TenantId });

    [HttpPost]
    [RequireIdempotency]
    [RequirePermission(PermissionCodes.SalesChannel.Payments.Create)]
    public IActionResult Create([FromBody] object? request) => Accepted(new { scaffold = true, resource = "payments", operation = "create", request, tenantId = TenantId });

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Payments.Read)]
    public IActionResult GetById(Guid id) => Ok(new { scaffold = true, resource = "payments", id, tenantId = TenantId });
}

[RequireRole("Admin", "Cashier")]
[Route("api/modules/sales-channel/returns")]
public sealed class ReturnsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SalesChannel.Returns.Read)]
    public IActionResult GetAll() => Ok(new { scaffold = true, resource = "returns", tenantId = TenantId });

    [HttpPost]
    [RequireIdempotency]
    [RequirePermission(PermissionCodes.SalesChannel.Returns.Create)]
    public IActionResult Create([FromBody] object? request) => Accepted(new { scaffold = true, resource = "returns", operation = "create", request, tenantId = TenantId });

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Returns.Read)]
    public IActionResult GetById(Guid id) => Ok(new { scaffold = true, resource = "returns", id, tenantId = TenantId });

    [HttpPatch("{id:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Returns.Update)]
    public IActionResult Patch(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "returns", id, operation = "patch", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/complete")]
    [RequirePermission(PermissionCodes.SalesChannel.Returns.Complete)]
    public IActionResult Complete(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "returns", id, action = "complete", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/cancel")]
    [RequirePermission(PermissionCodes.SalesChannel.Returns.Cancel)]
    public IActionResult Cancel(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "returns", id, action = "cancel", request, tenantId = TenantId });
}
