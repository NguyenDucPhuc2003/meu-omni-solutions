namespace MeuOmni.Modules.Suppliers.Domain.Suppliers;

public interface ISupplierRepository
{
    IQueryable<Supplier> Query();

    Task AddAsync(Supplier entity, CancellationToken cancellationToken = default);

    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ISupplierDebtTransactionRepository
{
    IQueryable<SupplierDebtTransaction> Query();

    Task AddAsync(SupplierDebtTransaction entity, CancellationToken cancellationToken = default);

    Task<SupplierDebtTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
