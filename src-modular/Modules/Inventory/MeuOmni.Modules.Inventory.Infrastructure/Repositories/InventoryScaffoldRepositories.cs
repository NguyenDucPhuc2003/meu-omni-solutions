using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Inventory.Domain.Inventory;
using MeuOmni.Modules.Inventory.Infrastructure.Persistence;

namespace MeuOmni.Modules.Inventory.Infrastructure.Repositories;

public sealed class StockTransactionRepository(InventoryDbContext dbContext) : IStockTransactionRepository
{
    public IQueryable<StockTransaction> Query()
    {
        return dbContext.Set<StockTransaction>().AsQueryable();
    }

    public async Task AddAsync(StockTransaction entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<StockTransaction>().AddAsync(entity, cancellationToken);
    }

    public Task<StockTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<StockTransaction>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class StockLevelRepository(InventoryDbContext dbContext) : IStockLevelRepository
{
    public IQueryable<StockLevel> Query()
    {
        return dbContext.Set<StockLevel>().AsQueryable();
    }

    public async Task AddAsync(StockLevel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<StockLevel>().AddAsync(entity, cancellationToken);
    }

    public Task<StockLevel?> GetByWarehouseProductAsync(Guid warehouseId, Guid productId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<StockLevel>()
            .FirstOrDefaultAsync(
                entity => entity.WarehouseId == warehouseId && entity.ProductId == productId,
                cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class StockCountSessionRepository(InventoryDbContext dbContext) : IStockCountSessionRepository
{
    public IQueryable<StockCountSession> Query()
    {
        return dbContext.Set<StockCountSession>().AsQueryable();
    }

    public async Task AddAsync(StockCountSession entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<StockCountSession>().AddAsync(entity, cancellationToken);
    }

    public Task<StockCountSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<StockCountSession>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
