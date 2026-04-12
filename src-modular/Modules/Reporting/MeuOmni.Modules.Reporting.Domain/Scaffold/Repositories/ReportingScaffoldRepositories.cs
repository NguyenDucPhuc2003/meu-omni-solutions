using MeuOmni.Modules.Reporting.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.Reporting.Domain.Scaffold.Repositories;

public interface ISalesDashboardReadModelRepository
{
    Task AddAsync(SalesDashboardReadModel entity, CancellationToken cancellationToken = default);

    Task<SalesDashboardReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SalesDashboardReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IShiftSummaryReadModelRepository
{
    Task AddAsync(ShiftSummaryReadModel entity, CancellationToken cancellationToken = default);

    Task<ShiftSummaryReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<ShiftSummaryReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface ISalesReportReadModelRepository
{
    Task AddAsync(SalesReportReadModel entity, CancellationToken cancellationToken = default);

    Task<SalesReportReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SalesReportReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IInventorySummaryReadModelRepository
{
    Task AddAsync(InventorySummaryReadModel entity, CancellationToken cancellationToken = default);

    Task<InventorySummaryReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<InventorySummaryReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface ICashFlowReadModelRepository
{
    Task AddAsync(CashFlowReadModel entity, CancellationToken cancellationToken = default);

    Task<CashFlowReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<CashFlowReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface ICustomerDebtReportReadModelRepository
{
    Task AddAsync(CustomerDebtReportReadModel entity, CancellationToken cancellationToken = default);

    Task<CustomerDebtReportReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<CustomerDebtReportReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface ISupplierDebtReportReadModelRepository
{
    Task AddAsync(SupplierDebtReportReadModel entity, CancellationToken cancellationToken = default);

    Task<SupplierDebtReportReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<SupplierDebtReportReadModel>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
