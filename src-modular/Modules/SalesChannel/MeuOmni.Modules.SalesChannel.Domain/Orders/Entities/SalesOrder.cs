using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Enums;

namespace MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;

/// <summary>
/// Represents a sales order from any channel (POS, Online, Hotline, etc.)
/// This is the core aggregate for the omni-channel sales system
/// </summary>
public sealed class SalesOrder : TenantAggregateRoot
{
    private SalesOrder()
    {
    }

    /// <summary>
    /// Creates a new sales order
    /// For POS channel, shiftId and cashierId are required
    /// For other channels, these can be null
    /// </summary>
    public SalesOrder(
        string tenantId, 
        SalesChannelType channel, 
        Guid? customerId,
        Guid? shiftId = null,
        Guid? cashierId = null,
        string? sourceOrderNumber = null)
    {
        InitializeTenant(tenantId);
        Channel = channel;
        CustomerId = customerId;
        ShiftId = shiftId;
        CashierId = cashierId;
        SourceOrderNumber = string.IsNullOrWhiteSpace(sourceOrderNumber) ? null : sourceOrderNumber.Trim();
        OrderNumber = GenerateOrderNumber(channel);
        Status = SalesOrderStatus.Draft;
    }

    public string OrderNumber { get; private set; } = string.Empty;

    public SalesChannelType Channel { get; private set; }

    public string? SourceOrderNumber { get; private set; }

    /// <summary>
    /// For POS orders, this is the shift where the sale happened
    /// Null for online/other channels
    /// </summary>
    public Guid? ShiftId { get; private set; }

    /// <summary>
    /// For POS orders, this is the cashier who processed the sale
    /// For other channels, could be the sales rep or null
    /// </summary>
    public Guid? CashierId { get; private set; }

    public Guid? CustomerId { get; private set; }

    public SalesOrderStatus Status { get; private set; }

    public decimal SubTotal { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal SurchargeAmount { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string? PriceAdjustmentReason { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public List<SalesOrderLine> Lines { get; private set; } = [];

    public List<PaymentEntry> Payments { get; private set; } = [];

    public void AddLine(Guid productId, string sku, string productName, decimal quantity, decimal unitPrice)
    {
        EnsureDraft();

        var existing = Lines.FirstOrDefault(x => x.ProductId == productId);
        if (existing is not null)
        {
            existing.Increase(quantity);
        }
        else
        {
            Lines.Add(new SalesOrderLine(TenantId, productId, sku, productName, quantity, unitPrice));
        }

        RecalculateTotals();
        Touch();
    }

    public void ApplyPriceAdjustment(decimal discountAmount, decimal surchargeAmount, string reason)
    {
        EnsureDraft();
        
        if (discountAmount < 0 || surchargeAmount < 0)
        {
            throw new DomainException("Adjustment amounts cannot be negative.");
        }

        if (discountAmount > SubTotal)
        {
            throw new DomainException("Discount amount cannot exceed subtotal.");
        }

        DiscountAmount = discountAmount;
        SurchargeAmount = surchargeAmount;
        PriceAdjustmentReason = string.IsNullOrWhiteSpace(reason) 
            ? throw new DomainException("Adjustment reason is required.") 
            : reason.Trim();
        
        RecalculateTotals();
        Touch();
    }

    public void RegisterPayment(PaymentMethod method, decimal amount, string? reference)
    {
        EnsureDraft();
        Payments.Add(new PaymentEntry(TenantId, method, amount, reference));
        Touch();
    }

    public void Submit()
    {
        EnsureDraft();

        if (Lines.Count == 0)
        {
            throw new DomainException("Sales order must contain at least one line before submitting.");
        }

        Status = SalesOrderStatus.Submitted;
        Touch();
    }

    public void Complete()
    {
        if (Status == SalesOrderStatus.Completed)
        {
            throw new DomainException("Order is already completed.");
        }

        if (Status == SalesOrderStatus.Cancelled)
        {
            throw new DomainException("Cancelled order cannot be completed.");
        }

        if (Lines.Count == 0)
        {
            throw new DomainException("Order must contain at least one line.");
        }

        // For POS orders, payment validation is stricter
        if (Channel == SalesChannelType.Pos)
        {
            var totalPaid = Payments.Sum(x => x.Amount);
            if (totalPaid != TotalAmount)
            {
                throw new DomainException("Total payment must match order total for POS orders.");
            }
        }

        Status = SalesOrderStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        Touch();
    }

    public void Cancel(string reason)
    {
        if (Status == SalesOrderStatus.Completed)
        {
            throw new DomainException("Completed order cannot be cancelled.");
        }

        if (Status == SalesOrderStatus.Cancelled)
        {
            throw new DomainException("Order is already cancelled.");
        }

        CancellationReason = string.IsNullOrWhiteSpace(reason) 
            ? throw new DomainException("Cancellation reason is required.") 
            : reason.Trim();
        
        Status = SalesOrderStatus.Cancelled;
        Touch();
    }

    private void EnsureDraft()
    {
        if (Status != SalesOrderStatus.Draft)
        {
            throw new DomainException("Only draft sales orders can be modified.");
        }
    }

    private void RecalculateTotals()
    {
        SubTotal = Lines.Sum(x => x.LineTotal);
        TotalAmount = SubTotal - DiscountAmount + SurchargeAmount;
    }

    private static string GenerateOrderNumber(SalesChannelType channel)
    {
        var prefix = channel switch
        {
            SalesChannelType.Pos => "POS",
            SalesChannelType.Online => "ONL",
            SalesChannelType.Hotline => "HTL",
            SalesChannelType.Facebook => "FB",
            SalesChannelType.Zalo => "ZL",
            SalesChannelType.Marketplace => "MKT",
            _ => "SO"
        };

        return $"{prefix}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..28];
    }
}
