using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Cashbook.Domain.Cashbooks;

public sealed class CashReconciliation : TenantAggregateRoot
{
    private CashReconciliation()
    {
    }

    public CashReconciliation(
        string tenantId,
        Guid cashbookId,
        decimal systemAmount,
        decimal countedAmount,
        string? differenceReason,
        string? confirmedBy)
    {
        InitializeTenant(tenantId);
        CashbookId = cashbookId;
        SystemAmount = systemAmount;
        CountedAmount = countedAmount;
        Difference = countedAmount - systemAmount;
        DifferenceReason = Normalize(differenceReason);
        ConfirmedBy = Normalize(confirmedBy);
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid CashbookId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal SystemAmount { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal CountedAmount { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Difference { get; private set; }

    public string? DifferenceReason { get; private set; }

    public string? ConfirmedBy { get; private set; }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
