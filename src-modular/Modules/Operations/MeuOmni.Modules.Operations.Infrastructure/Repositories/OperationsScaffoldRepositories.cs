using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Operations.Domain.Scaffold.Entities;
using MeuOmni.Modules.Operations.Domain.Scaffold.Repositories;
using MeuOmni.Modules.Operations.Infrastructure.Persistence;

namespace MeuOmni.Modules.Operations.Infrastructure.Repositories;

public sealed class DeviceRepository(OperationsDbContext dbContext) : IDeviceRepository
{
    public async Task AddAsync(Device entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Device>().AddAsync(entity, cancellationToken);
    }

    public Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Device>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<Device>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Device>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class PrinterRepository(OperationsDbContext dbContext) : IPrinterRepository
{
    public async Task AddAsync(Printer entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Printer>().AddAsync(entity, cancellationToken);
    }

    public Task<Printer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Printer>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<Printer>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Printer>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class StoreSettingRepository(OperationsDbContext dbContext) : IStoreSettingRepository
{
    public async Task AddAsync(StoreSetting entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<StoreSetting>().AddAsync(entity, cancellationToken);
    }

    public Task<StoreSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<StoreSetting>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<StoreSetting>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<StoreSetting>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class OperationalJobRepository(OperationsDbContext dbContext) : IOperationalJobRepository
{
    public async Task AddAsync(OperationalJob entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<OperationalJob>().AddAsync(entity, cancellationToken);
    }

    public Task<OperationalJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<OperationalJob>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<OperationalJob>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<OperationalJob>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
