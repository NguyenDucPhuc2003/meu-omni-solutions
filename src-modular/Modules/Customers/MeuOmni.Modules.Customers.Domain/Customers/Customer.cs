using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Customers.Domain.Customers;

public sealed class Customer : TenantAggregateRoot
{
    private Customer()
    {
    }

    public Customer(
        string tenantId,
        string fullName,
        string? code,
        string? phone,
        string? email,
        string? address,
        string? note)
    {
        InitializeTenant(tenantId);
        SetIdentity(code, fullName, phone, email);
        Address = Normalize(address);
        Note = Normalize(note);
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Code { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string FullName { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Phone { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Email { get; private set; }

    public string? Address { get; private set; }

    public string? Note { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal DebtBalance { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal TotalSpent { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; } = true;

    public void Update(string fullName, string? phone, string? email, string? address, string? note)
    {
        SetIdentity(Code, fullName, phone, email);
        Address = Normalize(address);
        Note = Normalize(note);
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

    public void ApplyDebtTransaction(CustomerDebtTransaction transaction)
    {
        DebtBalance += transaction.Type switch
        {
            CustomerDebtTransactionType.Increase => transaction.Amount,
            CustomerDebtTransactionType.Payment => transaction.Amount * -1,
            CustomerDebtTransactionType.Offset => transaction.Amount * -1,
            CustomerDebtTransactionType.Adjustment => transaction.Amount,
            _ => 0
        };

        Touch();
    }

    private void SetIdentity(string? code, string fullName, string? phone, string? email)
    {
        FullName = string.IsNullOrWhiteSpace(fullName)
            ? throw new DomainException("Customer full name is required.")
            : fullName.Trim();

        Code = Normalize(code);
        Phone = Normalize(phone);
        Email = Normalize(email);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
