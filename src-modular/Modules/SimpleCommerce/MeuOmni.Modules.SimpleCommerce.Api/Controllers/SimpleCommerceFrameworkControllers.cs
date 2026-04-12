using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;

namespace MeuOmni.Modules.SimpleCommerce.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/simple-commerce/public-catalog")]
public sealed class PublicCatalogController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SimpleCommerce.PublicCatalog.Read)]
    public IActionResult GetAll() => Ok(new { scaffold = true, resource = "public-catalog", tenantId = TenantId });
}

[RequireRole("Admin", "Cashier")]
[Route("api/modules/simple-commerce/checkout-sessions")]
public sealed class CheckoutSessionsController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SimpleCommerce.CheckoutSessions.Read)]
    public IActionResult GetAll() => Ok(new { scaffold = true, resource = "checkout-sessions", tenantId = TenantId });

    [HttpPost]
    [RequirePermission(PermissionCodes.SimpleCommerce.CheckoutSessions.Create)]
    public IActionResult Create([FromBody] object? request) => Accepted(new { scaffold = true, resource = "checkout-sessions", operation = "create", request, tenantId = TenantId });

    [HttpGet("{id:guid}")]
    [RequirePermission(PermissionCodes.SimpleCommerce.CheckoutSessions.Read)]
    public IActionResult GetById(Guid id) => Ok(new { scaffold = true, resource = "checkout-sessions", id, tenantId = TenantId });

    [HttpPatch("{id:guid}")]
    [RequirePermission(PermissionCodes.SimpleCommerce.CheckoutSessions.Update)]
    public IActionResult Patch(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "checkout-sessions", id, operation = "patch", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/complete")]
    [RequirePermission(PermissionCodes.SimpleCommerce.CheckoutSessions.Complete)]
    public IActionResult Complete(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "checkout-sessions", id, action = "complete", request, tenantId = TenantId });

    [HttpPost("{id:guid}/actions/cancel")]
    [RequirePermission(PermissionCodes.SimpleCommerce.CheckoutSessions.Cancel)]
    public IActionResult Cancel(Guid id, [FromBody] object? request) => Accepted(new { scaffold = true, resource = "checkout-sessions", id, action = "cancel", request, tenantId = TenantId });
}
