using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Cashbook.Domain.Cashbooks;

public sealed class Cashbook : TenantAggregateRoot
{
    private Cashbook()
    {
    }

    public Cashbook(string tenantId, string code, string name, string currencyCode, decimal openingBalance)
    {
        if (openingBalance < 0)
        {
            throw new DomainException("Opening balance cannot be negative.");
        }

        InitializeTenant(tenantId);
        Code = Require(code, "Cashbook code");
        Name = Require(name, "Cashbook name");
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "VND" : currencyCode.Trim().ToUpperInvariant();
        OpeningBalance = openingBalance;
        CurrentBalance = openingBalance;
        IsActive = true;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Code { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string CurrencyCode { get; private set; } = "VND";

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal OpeningBalance { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal CurrentBalance { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; }

    public void Update(string name, string currencyCode, bool isActive)
    {
        Name = Require(name, "Cashbook name");
        CurrencyCode = string.IsNullOrWhiteSpace(currencyCode) ? "VND" : currencyCode.Trim().ToUpperInvariant();
        IsActive = isActive;
        Touch();
    }

    public void ApplyTransaction(CashTransactionType type, decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Transaction amount must be greater than zero.");
        }

        CurrentBalance += type == CashTransactionType.Receipt ? amount : -amount;
        Touch();
    }

    public void ReverseTransaction(CashTransactionType type, decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Transaction amount must be greater than zero.");
        }

        CurrentBalance += type == CashTransactionType.Receipt ? -amount : amount;
        Touch();
    }

    public CashReconciliation Reconcile(decimal countedAmount, string? differenceReason, string? confirmedBy)
    {
        if (countedAmount < 0)
        {
            throw new DomainException("Counted amount cannot be negative.");
        }

        var reconciliation = new CashReconciliation(
            TenantId,
            Id,
            CurrentBalance,
            countedAmount,
            differenceReason,
            confirmedBy);

        CurrentBalance = countedAmount;
        Touch();
        return reconciliation;
    }

    private static string Require(string value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{fieldName} is required.")
            : value.Trim();
    }
}
