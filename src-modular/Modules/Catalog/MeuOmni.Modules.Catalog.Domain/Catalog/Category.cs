using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Catalog.Domain.Catalog;

public sealed class Category : TenantAggregateRoot
{
    private Category()
    {
    }

    public Category(string tenantId, string code, string name, Guid? parentId)
    {
        InitializeTenant(tenantId);
        Code = string.IsNullOrWhiteSpace(code) ? throw new DomainException("Category code is required.") : code.Trim();
        Name = string.IsNullOrWhiteSpace(name) ? throw new DomainException("Category name is required.") : name.Trim();
        ParentId = parentId;
        IsActive = true;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Code { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? ParentId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; }

    public void Update(string name, Guid? parentId)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new DomainException("Category name is required.") : name.Trim();
        ParentId = parentId;
        Touch();
    }
}
