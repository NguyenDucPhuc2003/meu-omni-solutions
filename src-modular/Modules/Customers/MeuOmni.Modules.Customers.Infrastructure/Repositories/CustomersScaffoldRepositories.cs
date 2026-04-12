using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Customers.Domain.Customers;
using MeuOmni.Modules.Customers.Infrastructure.Persistence;

namespace MeuOmni.Modules.Customers.Infrastructure.Repositories;

public sealed class CustomerRepository(CustomersDbContext dbContext) : ICustomerRepository
{
    public IQueryable<Customer> Query()
    {
        return dbContext.Set<Customer>().AsQueryable();
    }

    public async Task AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Customer>().AddAsync(entity, cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Customer>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class CustomerDebtTransactionRepository(CustomersDbContext dbContext) : ICustomerDebtTransactionRepository
{
    public IQueryable<CustomerDebtTransaction> Query()
    {
        return dbContext.Set<CustomerDebtTransaction>().AsQueryable();
    }

    public async Task AddAsync(CustomerDebtTransaction entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CustomerDebtTransaction>().AddAsync(entity, cancellationToken);
    }

    public Task<CustomerDebtTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CustomerDebtTransaction>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
