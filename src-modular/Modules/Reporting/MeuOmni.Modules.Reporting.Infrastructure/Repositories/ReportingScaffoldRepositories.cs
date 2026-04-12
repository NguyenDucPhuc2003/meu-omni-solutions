using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Reporting.Domain.Scaffold.Entities;
using MeuOmni.Modules.Reporting.Domain.Scaffold.Repositories;
using MeuOmni.Modules.Reporting.Infrastructure.Persistence;

namespace MeuOmni.Modules.Reporting.Infrastructure.Repositories;

public sealed class SalesDashboardReadModelRepository(ReportingDbContext dbContext) : ISalesDashboardReadModelRepository
{
    public async Task AddAsync(SalesDashboardReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<SalesDashboardReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<SalesDashboardReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SalesDashboardReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<SalesDashboardReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SalesDashboardReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class ShiftSummaryReadModelRepository(ReportingDbContext dbContext) : IShiftSummaryReadModelRepository
{
    public async Task AddAsync(ShiftSummaryReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<ShiftSummaryReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<ShiftSummaryReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ShiftSummaryReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<ShiftSummaryReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ShiftSummaryReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class SalesReportReadModelRepository(ReportingDbContext dbContext) : ISalesReportReadModelRepository
{
    public async Task AddAsync(SalesReportReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<SalesReportReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<SalesReportReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SalesReportReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<SalesReportReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SalesReportReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class InventorySummaryReadModelRepository(ReportingDbContext dbContext) : IInventorySummaryReadModelRepository
{
    public async Task AddAsync(InventorySummaryReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<InventorySummaryReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<InventorySummaryReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<InventorySummaryReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<InventorySummaryReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<InventorySummaryReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class CashFlowReadModelRepository(ReportingDbContext dbContext) : ICashFlowReadModelRepository
{
    public async Task AddAsync(CashFlowReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CashFlowReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<CashFlowReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CashFlowReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<CashFlowReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CashFlowReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class CustomerDebtReportReadModelRepository(ReportingDbContext dbContext) : ICustomerDebtReportReadModelRepository
{
    public async Task AddAsync(CustomerDebtReportReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CustomerDebtReportReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<CustomerDebtReportReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CustomerDebtReportReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<CustomerDebtReportReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<CustomerDebtReportReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class SupplierDebtReportReadModelRepository(ReportingDbContext dbContext) : ISupplierDebtReportReadModelRepository
{
    public async Task AddAsync(SupplierDebtReportReadModel entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<SupplierDebtReportReadModel>().AddAsync(entity, cancellationToken);
    }

    public Task<SupplierDebtReportReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SupplierDebtReportReadModel>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<SupplierDebtReportReadModel>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<SupplierDebtReportReadModel>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
