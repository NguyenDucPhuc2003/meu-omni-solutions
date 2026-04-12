namespace MeuOmni.Modules.Inventory.Domain.Inventory;

public interface IStockTransactionRepository
{
    IQueryable<StockTransaction> Query();
    Task AddAsync(StockTransaction entity, CancellationToken cancellationToken = default);
    Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IStockLevelRepository
{
    IQueryable<StockLevel> Query();
    Task AddAsync(StockLevel entity, CancellationToken cancellationToken = default);
    Task<StockLevel?> GetByWarehouseProductAsync(Guid warehouseId, Guid productId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IStockCountSessionRepository
{
    IQueryable<StockCountSession> Query();
    Task AddAsync(StockCountSession entity, CancellationToken cancellationToken = default);
    Task<StockCountSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
