using MeuOmni.Modules.Auditing.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.Auditing.Domain.Scaffold.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entity, CancellationToken cancellationToken = default);

    Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<AuditLogEntry>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
