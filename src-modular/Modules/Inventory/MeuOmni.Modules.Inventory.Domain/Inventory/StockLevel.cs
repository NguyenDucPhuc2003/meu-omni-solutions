using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Inventory.Domain.Inventory;

public sealed class StockLevel : TenantAggregateRoot
{
    private StockLevel()
    {
    }

    public StockLevel(string tenantId, Guid warehouseId, Guid productId)
    {
        InitializeTenant(tenantId);
        WarehouseId = warehouseId;
        ProductId = productId;
        OnHandQuantity = 0;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid WarehouseId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid ProductId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public decimal OnHandQuantity { get; private set; }

    public void Adjust(decimal delta)
    {
        OnHandQuantity += delta;
        Touch();
    }
}
