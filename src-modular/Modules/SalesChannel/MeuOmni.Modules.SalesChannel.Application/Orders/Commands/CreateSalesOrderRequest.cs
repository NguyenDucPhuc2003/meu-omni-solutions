using MeuOmni.Modules.SalesChannel.Domain.Orders.Enums;

namespace MeuOmni.Modules.SalesChannel.Application.Orders.Commands;

public sealed class CreateSalesOrderRequest
{
    public string TenantId { get; init; } = string.Empty;

    public SalesChannelType Channel { get; init; }

    public string? SourceOrderNumber { get; init; }

    public Guid? CustomerId { get; init; }

    public bool SubmitImmediately { get; init; } = true;

    public List<CreateSalesOrderLineRequest> Lines { get; init; } = [];
}

public sealed class CreateSalesOrderLineRequest
{
    public Guid ProductId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public decimal Quantity { get; init; }

    public decimal UnitPrice { get; init; }
}
