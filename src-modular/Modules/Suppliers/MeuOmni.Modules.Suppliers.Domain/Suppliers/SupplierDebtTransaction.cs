using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Suppliers.Domain.Suppliers;

public sealed class SupplierDebtTransaction : TenantAggregateRoot
{
    private SupplierDebtTransaction()
    {
    }

    public SupplierDebtTransaction(
        string tenantId,
        Guid supplierId,
        SupplierDebtTransactionType type,
        decimal amount,
        string? sourceDocumentType,
        Guid? sourceDocumentId,
        string? note)
    {
        if (amount <= 0)
        {
            throw new DomainException("Supplier debt transaction amount must be greater than zero.");
        }

        InitializeTenant(tenantId);
        SupplierId = supplierId;
        Type = type;
        Amount = amount;
        SourceDocumentType = Normalize(sourceDocumentType);
        SourceDocumentId = sourceDocumentId;
        Note = Normalize(note);
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid SupplierId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public SupplierDebtTransactionType Type { get; private set; }

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

public enum SupplierDebtTransactionType
{
    Increase = 1,
    Payment = 2,
    Offset = 3,
    Adjustment = 4
}
