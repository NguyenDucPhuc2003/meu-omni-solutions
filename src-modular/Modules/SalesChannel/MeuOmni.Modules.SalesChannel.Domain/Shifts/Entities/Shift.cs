using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.Modules.SalesChannel.Domain.Shifts.Enums;

namespace MeuOmni.Modules.SalesChannel.Domain.Shifts.Entities;

/// <summary>
/// Represents a cashier work shift for tracking cash register operations
/// </summary>
public sealed class Shift : TenantAggregateRoot
{
    private Shift()
    {
    }

    public Shift(string tenantId, Guid cashierId, string registerCode, decimal openingCash)
    {
        InitializeTenant(tenantId);
        if (openingCash < 0)
        {
            throw new DomainException("Opening cash cannot be negative.");
        }

        CashierId = cashierId;
        RegisterCode = string.IsNullOrWhiteSpace(registerCode) 
            ? throw new DomainException("Register code is required.") 
            : registerCode.Trim();
        OpeningCash = openingCash;
        Status = ShiftStatus.Open;
        OpenedAtUtc = DateTime.UtcNow;
    }

    public Guid CashierId { get; private set; }

    public string RegisterCode { get; private set; } = string.Empty;

    public decimal OpeningCash { get; private set; }

    public decimal ExpectedClosingCash { get; private set; }

    public decimal? ActualClosingCash { get; private set; }

    public decimal DiscrepancyAmount { get; private set; }

    public string? DiscrepancyReason { get; private set; }

    public ShiftStatus Status { get; private set; }

    public DateTime OpenedAtUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    public void UpdateExpectedClosingCash(decimal expectedClosingCash)
    {
        if (Status != ShiftStatus.Open)
        {
            throw new DomainException("Only open shifts can be updated.");
        }

        ExpectedClosingCash = expectedClosingCash;
        Touch();
    }

    public void Close(decimal actualClosingCash, decimal expectedClosingCash, string? discrepancyReason)
    {
        if (Status != ShiftStatus.Open)
        {
            throw new DomainException("Shift is already closed.");
        }

        ActualClosingCash = actualClosingCash;
        ExpectedClosingCash = expectedClosingCash;
        DiscrepancyAmount = actualClosingCash - expectedClosingCash;
        
        if (DiscrepancyAmount != 0 && string.IsNullOrWhiteSpace(discrepancyReason))
        {
            throw new DomainException("Discrepancy reason is required when there is cash mismatch.");
        }

        DiscrepancyReason = discrepancyReason?.Trim();
        Status = ShiftStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;
        Touch();
    }
}
