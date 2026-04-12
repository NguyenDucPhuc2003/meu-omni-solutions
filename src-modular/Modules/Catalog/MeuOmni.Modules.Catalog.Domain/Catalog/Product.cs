using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Catalog.Domain.Catalog;

public sealed class Product : TenantAggregateRoot
{
    private Product()
    {
    }

    public Product(
        string tenantId,
        string name,
        string? code,
        string? sku,
        string? barcode,
        Guid? categoryId,
        decimal sellPrice)
    {
        InitializeTenant(tenantId);
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("Product name is required.")
            : name.Trim();
        Code = Normalize(code);
        Sku = Normalize(sku);
        Barcode = Normalize(barcode);
        CategoryId = categoryId;
        SellPrice = sellPrice;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Code { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Sku { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string? Barcode { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string Name { get; private set; } = string.Empty;

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? CategoryId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal SellPrice { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; } = true;

    public void Update(string name, Guid? categoryId, decimal sellPrice)
    {
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("Product name is required.")
            : name.Trim();
        CategoryId = categoryId;
        SellPrice = sellPrice;
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

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
