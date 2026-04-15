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
        Guid? storeId,
        string fullName,
        string? code,
        Guid? groupId,
        string customerType,
        string? companyName,
        string? taxCode,
        string? phone,
        string? email,
        string? gender,
        DateOnly? birthday,
        string? addressLine,
        string? ward,
        string? district,
        string? city,
        string? note)
    {
        InitializeTenant(tenantId);
        StoreId = storeId;
        GroupId = groupId;
        SetIdentity(code, fullName, customerType, companyName, taxCode, phone, email, gender, birthday);
        SetAddress(addressLine, ward, district, city);
        Note = Normalize(note);
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? StoreId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Code { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? GroupId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string FullName { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string CustomerType { get; private set; } = "INDIVIDUAL";

    public string? CompanyName { get; private set; }

    public string? TaxCode { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Phone { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Email { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Gender { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public DateOnly? Birthday { get; private set; }

    public string? AddressLine { get; private set; }

    public string? Ward { get; private set; }

    public string? District { get; private set; }

    public string? City { get; private set; }

    public string? Note { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal DebtBalance { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal TotalSpent { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; } = true;

    public void Update(
        string fullName,
        Guid? groupId,
        string customerType,
        string? companyName,
        string? taxCode,
        string? phone,
        string? email,
        string? gender,
        DateOnly? birthday,
        string? addressLine,
        string? ward,
        string? district,
        string? city,
        string? note)
    {
        GroupId = groupId;
        SetIdentity(Code, fullName, customerType, companyName, taxCode, phone, email, gender, birthday);
        SetAddress(addressLine, ward, district, city);
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
        var delta = transaction.Type switch
        {
            CustomerDebtTransactionType.Increase => transaction.Amount,
            CustomerDebtTransactionType.Payment => transaction.Amount * -1,
            CustomerDebtTransactionType.Offset => transaction.Amount * -1,
            CustomerDebtTransactionType.Adjustment => transaction.Amount,
            _ => 0
        };

        if (DebtBalance + delta < 0)
        {
            throw new DomainException("Customer payment cannot exceed debt balance.");
        }

        DebtBalance += delta;

        Touch();
    }

    private void SetIdentity(
        string? code,
        string fullName,
        string customerType,
        string? companyName,
        string? taxCode,
        string? phone,
        string? email,
        string? gender,
        DateOnly? birthday)
    {
        FullName = string.IsNullOrWhiteSpace(fullName)
            ? throw new DomainException("Customer full name is required.")
            : fullName.Trim();

        Code = Normalize(code);
        CustomerType = ValidateCustomerType(customerType);
        CompanyName = Normalize(companyName);
        TaxCode = Normalize(taxCode);
        Phone = Normalize(phone);
        Email = Normalize(email);
        Gender = ValidateGender(gender);
        Birthday = birthday;
    }

    private void SetAddress(string? addressLine, string? ward, string? district, string? city)
    {
        AddressLine = Normalize(addressLine);
        Ward = Normalize(ward);
        District = Normalize(district);
        City = Normalize(city);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string ValidateCustomerType(string customerType)
    {
        var normalized = string.IsNullOrWhiteSpace(customerType)
            ? "INDIVIDUAL"
            : customerType.Trim().ToUpperInvariant();

        return normalized is "INDIVIDUAL" or "BUSINESS"
            ? normalized
            : throw new DomainException("Customer type must be either INDIVIDUAL or BUSINESS.");
    }

    private static string? ValidateGender(string? gender)
    {
        var normalized = Normalize(gender);
        if (normalized is null)
        {
            return null;
        }

        var upper = normalized.ToUpperInvariant();
        return upper is "MALE" or "FEMALE" or "OTHER"
            ? upper
            : throw new DomainException("Gender must be MALE, FEMALE, or OTHER.");
    }
}
