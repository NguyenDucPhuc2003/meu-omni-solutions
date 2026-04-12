using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;

/// <summary>
/// Represents a line item in a sales order
/// </summary>
public sealed class SalesOrderLine : TenantEntity
{
    private SalesOrderLine()
    {
    }

    public SalesOrderLine(string tenantId, Guid productId, string sku, string productName, decimal quantity, decimal unitPrice)
    {
        InitializeTenant(tenantId);
        ProductId = productId;
        Sku = Require(sku, "SKU");
        ProductName = Require(productName, "Product name");

        if (quantity <= 0)
        {
            throw new DomainException("Order line quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new DomainException("Unit price cannot be negative.");
        }

        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public Guid SalesOrderId { get; private set; }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    public void Increase(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Increase quantity must be greater than zero.");
        }

        Quantity += quantity;
        Touch();
    }

    private static string Require(string value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{fieldName} is required.")
            : value.Trim();
    }
}

