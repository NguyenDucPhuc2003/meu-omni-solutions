using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Suppliers.Domain.Suppliers;
using MeuOmni.Modules.Suppliers.Infrastructure.Persistence;

namespace MeuOmni.Modules.Suppliers.Infrastructure.Repositories;

public sealed class SupplierRepository(SuppliersDbContext dbContext) : ISupplierRepository
{
    public IQueryable<Supplier> Query()
    {
        return dbContext.Set<Supplier>().AsQueryable();
    }

    public async Task AddAsync(Supplier entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Supplier>().AddAsync(entity, cancellationToken);
    }

    public Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Supplier>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class SupplierDebtTransactionRepository(SuppliersDbContext dbContext) : ISupplierDebtTransactionRepository
{
    public IQueryable<SupplierDebtTransaction> Query()
    {
        return dbContext.Set<SupplierDebtTransaction>().AsQueryable();
    }

    public async Task AddAsync(SupplierDebtTransaction entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<SupplierDebtTransaction>().AddAsync(entity, cancellationToken);
    }

    public Task<SupplierDebtTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SupplierDebtTransaction>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
