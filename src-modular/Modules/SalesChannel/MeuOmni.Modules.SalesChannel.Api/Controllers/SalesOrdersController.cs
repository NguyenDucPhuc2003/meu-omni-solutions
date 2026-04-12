using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.SalesChannel.Application.Orders.Commands;
using MeuOmni.Modules.SalesChannel.Application.Orders.Services;

namespace MeuOmni.Modules.SalesChannel.Api.Controllers;

[RequireRole("Admin", "Cashier")]
[Route("api/modules/sales-channel/orders")]
public sealed class SalesOrdersController(
    ISalesOrderApplicationService salesOrderApplicationService,
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{
    [HttpGet]
    [RequirePermission(PermissionCodes.SalesChannel.Orders.Read)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await salesOrderApplicationService.ListAsync(cancellationToken));
    }

    [HttpGet("{orderId:guid}")]
    [RequirePermission(PermissionCodes.SalesChannel.Orders.Read)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await salesOrderApplicationService.GetByIdAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.SalesChannel.Orders.Create)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request, CancellationToken cancellationToken)
    {
        return Ok(await salesOrderApplicationService.CreateAsync(request, cancellationToken));
    }
}
