namespace MeuOmni.Modules.Customers.Domain.Customers;

public interface ICustomerRepository
{
    IQueryable<Customer> Query();

    Task<bool> ExistsByCodeAsync(string tenantId, string code, CancellationToken cancellationToken = default);

    Task<bool> ExistsByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);

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

public interface ICustomerGroupRepository
{
    IQueryable<CustomerGroup> Query();

    Task<bool> ExistsByCodeAsync(string tenantId, string code, CancellationToken cancellationToken = default);

    Task AddAsync(CustomerGroup entity, CancellationToken cancellationToken = default);

    Task<CustomerGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Remove(CustomerGroup entity);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
