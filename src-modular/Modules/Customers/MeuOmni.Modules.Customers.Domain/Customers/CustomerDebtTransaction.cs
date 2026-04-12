using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Customers.Domain.Customers;

public sealed class CustomerDebtTransaction : TenantAggregateRoot
{
    private CustomerDebtTransaction()
    {
    }

    public CustomerDebtTransaction(
        string tenantId,
        Guid customerId,
        CustomerDebtTransactionType type,
        decimal amount,
        string? sourceDocumentType,
        Guid? sourceDocumentId,
        string? note)
    {
        if (amount <= 0)
        {
            throw new DomainException("Debt transaction amount must be greater than zero.");
        }

        InitializeTenant(tenantId);
        CustomerId = customerId;
        Type = type;
        Amount = amount;
        SourceDocumentType = Normalize(sourceDocumentType);
        SourceDocumentId = sourceDocumentId;
        Note = Normalize(note);
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid CustomerId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public CustomerDebtTransactionType Type { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Amount { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? SourceDocumentType { get; private set; }

    public Guid? SourceDocumentId { get; private set; }

    public string? Note { get; private set; }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public enum CustomerDebtTransactionType
{
    Increase = 1,
    Payment = 2,
    Offset = 3,
    Adjustment = 4
}
