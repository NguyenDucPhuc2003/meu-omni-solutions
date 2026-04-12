using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Inventory.Domain.Inventory;

public enum StockTransactionType
{
    PurchaseIn = 1,
    SaleOut = 2,
    ReturnIn = 3,
    AdjustIn = 4,
    AdjustOut = 5,
    Transfer = 6
}

public enum StockTransactionStatus
{
    Active = 1,
    Cancelled = 2
}

public sealed class StockTransaction : TenantAggregateRoot
{
    private readonly List<StockTransactionItem> items = [];

    private StockTransaction()
    {
    }

    public StockTransaction(
        string tenantId,
        StockTransactionType type,
        Guid? warehouseId,
        Guid? fromWarehouseId,
        Guid? toWarehouseId,
        string? note)
    {
        InitializeTenant(tenantId);
        Type = type;
        WarehouseId = warehouseId;
        FromWarehouseId = fromWarehouseId;
        ToWarehouseId = toWarehouseId;
        Note = Normalize(note);
        Status = StockTransactionStatus.Active;

        if (type == StockTransactionType.Transfer)
        {
            if (fromWarehouseId is null || toWarehouseId is null)
            {
                throw new DomainException("Transfer transaction requires both source and destination warehouse.");
            }

            if (fromWarehouseId == toWarehouseId)
            {
                throw new DomainException("Transfer warehouses must be different.");
            }
        }
        else if (warehouseId is null)
        {
            throw new DomainException("Warehouse is required for this stock transaction type.");
        }
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public StockTransactionType Type { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? WarehouseId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? FromWarehouseId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid? ToWarehouseId { get; private set; }

    public string? Note { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public StockTransactionStatus Status { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<StockTransactionItem> Items => items;

    public void AddItem(Guid productId, decimal quantity, decimal? unitCost)
    {
        EnsureActive();

        if (quantity <= 0)
        {
            throw new DomainException("Transaction item quantity must be greater than zero.");
        }

        items.Add(new StockTransactionItem(TenantId, productId, quantity, unitCost));
        Touch();
    }

    public void Cancel(string? reason)
    {
        EnsureActive();
        CancellationReason = Normalize(reason);
        Status = StockTransactionStatus.Cancelled;
        Touch();
    }

    private void EnsureActive()
    {
        if (Status == StockTransactionStatus.Cancelled)
        {
            throw new DomainException("Cancelled stock transaction cannot be modified.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public sealed class StockTransactionItem : TenantEntity
{
    private StockTransactionItem()
    {
    }

    public StockTransactionItem(string tenantId, Guid productId, decimal quantity, decimal? unitCost)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Quantity must be greater than zero.");
        }

        InitializeTenant(tenantId);
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
    }

    public Guid StockTransactionId { get; private set; }

    public Guid ProductId { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal? UnitCost { get; private set; }
}
