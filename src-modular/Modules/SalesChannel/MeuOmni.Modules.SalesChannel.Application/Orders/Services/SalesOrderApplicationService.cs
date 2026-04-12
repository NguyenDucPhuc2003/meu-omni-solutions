using MeuOmni.Modules.SalesChannel.Application.Orders.Commands;
using MeuOmni.Modules.SalesChannel.Application.Orders.Dtos;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Repositories;

namespace MeuOmni.Modules.SalesChannel.Application.Orders.Services;

public sealed class SalesOrderApplicationService(
    ISalesOrderRepository salesOrderRepository,
    ITenantContextAccessor tenantContextAccessor) : ISalesOrderApplicationService
{
    public async Task<SalesOrderDto> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);

        var order = new SalesOrder(
            tenantId,
            request.Channel, 
            request.CustomerId,
            shiftId: null,
            cashierId: null,
            sourceOrderNumber: request.SourceOrderNumber);

        foreach (var line in request.Lines)
        {
            order.AddLine(line.ProductId, line.Sku, line.ProductName, line.Quantity, line.UnitPrice);
        }

        if (request.SubmitImmediately)
        {
            order.Submit();
        }

        await salesOrderRepository.AddAsync(order, cancellationToken);
        await salesOrderRepository.SaveChangesAsync(cancellationToken);

        return ToDto(order);
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await salesOrderRepository.GetByIdAsync(orderId, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    public async Task<List<SalesOrderDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var orders = await salesOrderRepository.ListAsync(cancellationToken);
        return orders.Select(ToDto).ToList();
    }

    private static SalesOrderDto ToDto(SalesOrder order)
    {
        return new SalesOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TenantId = order.TenantId,
            Channel = order.Channel.ToString(),
            Status = order.Status.ToString(),
            SourceOrderNumber = order.SourceOrderNumber,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Lines = order.Lines.Select(x => new SalesOrderLineDto
            {
                ProductId = x.ProductId,
                Sku = x.Sku,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.LineTotal
            }).ToList()
        };
    }
}
