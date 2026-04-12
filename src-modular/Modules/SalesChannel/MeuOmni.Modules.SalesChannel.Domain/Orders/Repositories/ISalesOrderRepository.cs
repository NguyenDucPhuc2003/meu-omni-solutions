using MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;

namespace MeuOmni.Modules.SalesChannel.Domain.Orders.Repositories;

public interface ISalesOrderRepository
{
    Task AddAsync(SalesOrder order, CancellationToken cancellationToken = default);

    Task<SalesOrder?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<List<SalesOrder>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
