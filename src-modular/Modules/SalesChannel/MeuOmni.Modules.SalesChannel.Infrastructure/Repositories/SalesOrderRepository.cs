using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Repositories;
using MeuOmni.Modules.SalesChannel.Infrastructure.Persistence;

namespace MeuOmni.Modules.SalesChannel.Infrastructure.Repositories;

public sealed class SalesOrderRepository(SalesChannelDbContext dbContext) : ISalesOrderRepository
{
    public async Task AddAsync(SalesOrder order, CancellationToken cancellationToken = default)
    {
        await dbContext.SalesOrders.AddAsync(order, cancellationToken);
    }

    public Task<SalesOrder?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return dbContext.SalesOrders.FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);
    }

    public Task<List<SalesOrder>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SalesOrders
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
