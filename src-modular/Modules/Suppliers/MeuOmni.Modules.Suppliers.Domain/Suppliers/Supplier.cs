using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Suppliers.Domain.Suppliers;

public sealed class Supplier : TenantAggregateRoot
{
    private Supplier()
    {
    }

    public Supplier(
        string tenantId,
        string name,
        string? code,
        string? phone,
        string? email,
        string? address,
        string? contactPerson)
    {
        InitializeTenant(tenantId);
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("Supplier name is required.")
            : name.Trim();
        Code = Normalize(code);
        Phone = Normalize(phone);
        Email = Normalize(email);
        Address = Normalize(address);
        ContactPerson = Normalize(contactPerson);
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Code { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Phone { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Email { get; private set; }

    public string? Address { get; private set; }

    public string? ContactPerson { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal DebtBalance { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; } = true;

    public void Update(string name, string? phone, string? email, string? address, string? contactPerson)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("Supplier name is required.")
            : name.Trim();
        Phone = Normalize(phone);
        Email = Normalize(email);
        Address = Normalize(address);
        ContactPerson = Normalize(contactPerson);
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void ApplyDebt(SupplierDebtTransaction transaction)
    {
        DebtBalance += transaction.Type switch
        {
            SupplierDebtTransactionType.Increase => transaction.Amount,
            SupplierDebtTransactionType.Adjustment => transaction.Amount,
            SupplierDebtTransactionType.Payment => transaction.Amount * -1,
            SupplierDebtTransactionType.Offset => transaction.Amount * -1,
            _ => 0
        };

        Touch();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
