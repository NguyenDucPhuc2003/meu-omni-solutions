namespace MeuOmni.Modules.Customers.Domain.Customers;

public interface ICustomerRepository
{
    IQueryable<Customer> Query();

    Task AddAsync(Customer entity, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICustomerDebtTransactionRepository
{
    IQueryable<CustomerDebtTransaction> Query();

    Task AddAsync(CustomerDebtTransaction entity, CancellationToken cancellationToken = default);

    Task<CustomerDebtTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
