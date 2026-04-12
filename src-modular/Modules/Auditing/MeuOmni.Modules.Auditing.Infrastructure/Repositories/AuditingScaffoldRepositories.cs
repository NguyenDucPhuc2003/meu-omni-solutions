using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Auditing.Domain.Scaffold.Entities;
using MeuOmni.Modules.Auditing.Domain.Scaffold.Repositories;
using MeuOmni.Modules.Auditing.Infrastructure.Persistence;

namespace MeuOmni.Modules.Auditing.Infrastructure.Repositories;

public sealed class AuditLogRepository(AuditingDbContext dbContext) : IAuditLogRepository
{
    public async Task AddAsync(AuditLogEntry entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<AuditLogEntry>().AddAsync(entity, cancellationToken);
    }

    public Task<AuditLogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<AuditLogEntry>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<AuditLogEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<AuditLogEntry>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
