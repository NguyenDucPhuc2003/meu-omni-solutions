using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Catalog.Domain.Catalog;

public sealed class ProductPrice : TenantAggregateRoot
{
    private ProductPrice()
    {
    }

    public ProductPrice(
        string tenantId,
        Guid productId,
        string priceType,
        decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        InitializeTenant(tenantId);
        ProductId = productId;
        PriceType = string.IsNullOrWhiteSpace(priceType) ? "DEFAULT" : priceType.Trim().ToUpperInvariant();
        Price = price;
        IsActive = true;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid ProductId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string PriceType { get; private set; } = "DEFAULT";

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal Price { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public bool IsActive { get; private set; }

    public void Update(decimal price)
    {
        if (price < 0)
        {
            throw new DomainException("Price cannot be negative.");
        }

        Price = price;
        Touch();
    }
}
