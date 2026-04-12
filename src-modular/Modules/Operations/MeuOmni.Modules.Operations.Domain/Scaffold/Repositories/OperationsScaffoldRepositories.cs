using MeuOmni.Modules.Operations.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.Operations.Domain.Scaffold.Repositories;

public interface IDeviceRepository
{
    Task AddAsync(Device entity, CancellationToken cancellationToken = default);

    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Device>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IPrinterRepository
{
    Task AddAsync(Printer entity, CancellationToken cancellationToken = default);

    Task<Printer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Printer>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IStoreSettingRepository
{
    Task AddAsync(StoreSetting entity, CancellationToken cancellationToken = default);

    Task<StoreSetting?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<StoreSetting>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IOperationalJobRepository
{
    Task AddAsync(OperationalJob entity, CancellationToken cancellationToken = default);

    Task<OperationalJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<OperationalJob>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
