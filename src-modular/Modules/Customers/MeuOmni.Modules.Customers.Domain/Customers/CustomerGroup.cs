using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Customers.Domain.Customers;

public sealed class CustomerGroup : TenantAggregateRoot
{
    private CustomerGroup()
    {
    }

    public CustomerGroup(string tenantId, string code, string name, string? description)
    {
        InitializeTenant(tenantId);
        SetIdentity(code, name, description);
        IsActive = true;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Code { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; }

    public void Update(string name, string? description)
    {
        Name = Require(name, "Customer group name");
        Description = Normalize(description);
        Touch();
    }

    private void SetIdentity(string code, string name, string? description)
    {
        Code = Require(code, "Customer group code");
        Name = Require(name, "Customer group name");
        Description = Normalize(description);
    }

    private static string Require(string value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{fieldName} is required.")
            : value.Trim();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
