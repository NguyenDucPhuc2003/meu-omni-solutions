using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Cashbook.Domain.Cashbooks;

public enum CashTransactionType
{
    Receipt = 1,
    Payment = 2
}

public enum CashTransactionStatus
{
    Active = 1,
    Cancelled = 2
}

public sealed class CashTransaction : TenantAggregateRoot
{
    private CashTransaction()
    {
    }

    public CashTransaction(
        string tenantId,
        Guid cashbookId,
        CashTransactionType type,
        string? subType,
        string? paymentMethod,
        decimal amount,
        string? counterpartyType,
        Guid? counterpartyId,
        string? sourceDocumentType,
        Guid? sourceDocumentId,
        string? note)
    {
        if (amount <= 0)
        {
            throw new DomainException("Amount must be greater than zero.");
        }

        InitializeTenant(tenantId);
        CashbookId = cashbookId;
        Type = type;
        SubType = Normalize(subType);
        PaymentMethod = Normalize(paymentMethod);
        Amount = amount;
        CounterpartyType = Normalize(counterpartyType);
        CounterpartyId = counterpartyId;
        SourceDocumentType = Normalize(sourceDocumentType);
        SourceDocumentId = sourceDocumentId;
        Note = Normalize(note);
        Status = CashTransactionStatus.Active;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid CashbookId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public CashTransactionType Type { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? SubType { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? PaymentMethod { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Amount { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? CounterpartyType { get; private set; }

    public Guid? CounterpartyId { get; private set; }

    public string? SourceDocumentType { get; private set; }

    public Guid? SourceDocumentId { get; private set; }

    public string? Note { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public CashTransactionStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public void Update(string? paymentMethod, string? note)
    {
        EnsureActive();
        PaymentMethod = Normalize(paymentMethod);
        Note = Normalize(note);
        Touch();
    }

    public void Cancel(string? reason)
    {
        EnsureActive();
        CancellationReason = Normalize(reason);
        Status = CashTransactionStatus.Cancelled;
        Touch();
    }

    private void EnsureActive()
    {
        if (Status == CashTransactionStatus.Cancelled)
        {
            throw new DomainException("Cancelled cash transaction cannot be modified.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
