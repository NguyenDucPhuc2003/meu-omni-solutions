namespace MeuOmni.Modules.Cashbook.Domain.Cashbooks;

public interface ICashbookRepository
{
    IQueryable<Cashbook> Query();
    Task AddAsync(Cashbook entity, CancellationToken cancellationToken = default);
    Task<Cashbook?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICashTransactionRepository
{
    IQueryable<CashTransaction> Query();
    Task AddAsync(CashTransaction entity, CancellationToken cancellationToken = default);
    Task<CashTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICashReconciliationRepository
{
    IQueryable<CashReconciliation> Query();
    Task AddAsync(CashReconciliation entity, CancellationToken cancellationToken = default);
    Task<CashReconciliation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
