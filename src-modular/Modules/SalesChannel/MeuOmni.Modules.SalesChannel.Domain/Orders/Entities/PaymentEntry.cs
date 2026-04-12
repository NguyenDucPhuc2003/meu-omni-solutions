using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Enums;

namespace MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;

/// <summary>
/// Represents a payment entry for a sales order
/// </summary>
public sealed class PaymentEntry : TenantEntity
{
    private PaymentEntry()
    {
    }

    public PaymentEntry(string tenantId, PaymentMethod method, decimal amount, string? reference)
    {
        InitializeTenant(tenantId);
        if (amount <= 0)
        {
            throw new DomainException("Payment amount must be greater than zero.");
        }

        Method = method;
        Amount = amount;
        Reference = reference?.Trim();
    }

    public Guid SalesOrderId { get; private set; }

    public PaymentMethod Method { get; private set; }

    public decimal Amount { get; private set; }

    public string? Reference { get; private set; }
}
