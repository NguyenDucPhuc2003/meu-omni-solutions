using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Cashbook.Domain.Cashbooks;
using MeuOmni.Modules.Cashbook.Infrastructure.Persistence;
using CashbookAggregate = MeuOmni.Modules.Cashbook.Domain.Cashbooks.Cashbook;

namespace MeuOmni.Modules.Cashbook.Infrastructure.Repositories;

public sealed class CashbookRepository(CashbookDbContext dbContext) : ICashbookRepository
{
    public IQueryable<CashbookAggregate> Query()
    {
        return dbContext.Set<CashbookAggregate>().AsQueryable();
    }

    public async Task AddAsync(CashbookAggregate entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CashbookAggregate>().AddAsync(entity, cancellationToken);
    }

    public Task<CashbookAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CashbookAggregate>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class CashTransactionRepository(CashbookDbContext dbContext) : ICashTransactionRepository
{
    public IQueryable<CashTransaction> Query()
    {
        return dbContext.Set<CashTransaction>().AsQueryable();
    }

    public async Task AddAsync(CashTransaction entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CashTransaction>().AddAsync(entity, cancellationToken);
    }

    public Task<CashTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CashTransaction>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class CashReconciliationRepository(CashbookDbContext dbContext) : ICashReconciliationRepository
{
    public IQueryable<CashReconciliation> Query()
    {
        return dbContext.Set<CashReconciliation>().AsQueryable();
    }

    public async Task AddAsync(CashReconciliation entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CashReconciliation>().AddAsync(entity, cancellationToken);
    }

    public Task<CashReconciliation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CashReconciliation>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
