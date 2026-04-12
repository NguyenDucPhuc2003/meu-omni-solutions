using MeuOmni.Modules.SalesChannel.Application.Orders.Commands;
using MeuOmni.Modules.SalesChannel.Application.Orders.Dtos;

namespace MeuOmni.Modules.SalesChannel.Application.Orders.Services;

public interface ISalesOrderApplicationService
{
    Task<SalesOrderDto> CreateAsync(CreateSalesOrderRequest request, CancellationToken cancellationToken = default);

    Task<SalesOrderDto?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<List<SalesOrderDto>> ListAsync(CancellationToken cancellationToken = default);
}
