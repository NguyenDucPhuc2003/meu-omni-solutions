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

    public Task<bool> ExistsByCodeAsync(string tenantId, string code, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Customer>()
            .AnyAsync(entity => entity.TenantId == tenantId && entity.Code != null && entity.Code == code, cancellationToken);
    }

    public Task<bool> ExistsByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Customer>()
            .AnyAsync(entity => entity.GroupId == groupId, cancellationToken);
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

public sealed class CustomerGroupRepository(CustomersDbContext dbContext) : ICustomerGroupRepository
{
    public IQueryable<CustomerGroup> Query()
    {
        return dbContext.Set<CustomerGroup>().AsQueryable();
    }

    public Task<bool> ExistsByCodeAsync(string tenantId, string code, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CustomerGroup>()
            .AnyAsync(entity => entity.TenantId == tenantId && entity.Code == code, cancellationToken);
    }

    public async Task AddAsync(CustomerGroup entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CustomerGroup>().AddAsync(entity, cancellationToken);
    }

    public Task<CustomerGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CustomerGroup>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public void Remove(CustomerGroup entity)
    {
        dbContext.Set<CustomerGroup>().Remove(entity);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
