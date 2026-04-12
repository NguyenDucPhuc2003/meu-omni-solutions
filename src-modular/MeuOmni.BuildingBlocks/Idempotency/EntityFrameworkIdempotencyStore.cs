using Microsoft.EntityFrameworkCore;

namespace MeuOmni.BuildingBlocks.Idempotency;

public sealed class EntityFrameworkIdempotencyStore<TDbContext>(TDbContext dbContext) : IIdempotencyStore
    where TDbContext : DbContext
{
    private static readonly TimeSpan PendingTimeout = TimeSpan.FromMinutes(10);

    public async Task<IdempotencyReservationResult> ReserveAsync(
        string tenantId,
        string requestMethod,
        string requestPath,
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindAsync(tenantId, requestMethod, requestPath, idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return await ResolveExistingAsync(existing, requestHash, cancellationToken);
        }

        var record = IdempotencyRequestRecord.Reserve(tenantId, requestMethod, requestPath, idempotencyKey, requestHash);
        dbContext.Set<IdempotencyRequestRecord>().Add(record);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return new IdempotencyReservationResult(IdempotencyReservationStatus.Reserved, record);
        }
        catch (DbUpdateException)
        {
            dbContext.Entry(record).State = EntityState.Detached;

            existing = await FindAsync(tenantId, requestMethod, requestPath, idempotencyKey, cancellationToken);
            if (existing is null)
            {
                throw;
            }

            return await ResolveExistingAsync(existing, requestHash, cancellationToken);
        }
    }

    public async Task CompleteAsync(
        IdempotencyRequestRecord record,
        int statusCode,
        string? contentType,
        string? responseBody,
        CancellationToken cancellationToken = default)
    {
        record.Complete(statusCode, contentType, responseBody);
        DetachNonIdempotencyEntries(record);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseAsync(
        IdempotencyRequestRecord record,
        CancellationToken cancellationToken = default)
    {
        DetachNonIdempotencyEntries(record);
        dbContext.Set<IdempotencyRequestRecord>().Remove(record);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task<IdempotencyRequestRecord?> FindAsync(
        string tenantId,
        string requestMethod,
        string requestPath,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<IdempotencyRequestRecord>()
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId
                    && x.RequestMethod == requestMethod
                    && x.RequestPath == requestPath
                    && x.IdempotencyKey == idempotencyKey,
                cancellationToken);
    }

    private async Task<IdempotencyReservationResult> ResolveExistingAsync(
        IdempotencyRequestRecord existing,
        string requestHash,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
        {
            return new IdempotencyReservationResult(
                IdempotencyReservationStatus.Conflict,
                existing,
                "The supplied idempotency key was already used for a different request payload.");
        }

        if (existing.IsCompleted)
        {
            return new IdempotencyReservationResult(IdempotencyReservationStatus.Replay, existing);
        }

        if (existing.IsExpired(PendingTimeout, DateTime.UtcNow))
        {
            await ReleaseAsync(existing, cancellationToken);

            return await ReserveAsync(
                existing.TenantId,
                existing.RequestMethod,
                existing.RequestPath,
                existing.IdempotencyKey,
                requestHash,
                cancellationToken);
        }

        return new IdempotencyReservationResult(
            IdempotencyReservationStatus.InProgress,
            existing,
            "A request with the same Idempotency-Key is already being processed.");
    }

    private void DetachNonIdempotencyEntries(IdempotencyRequestRecord currentRecord)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries().Where(x => x.Entity is not IdempotencyRequestRecord))
        {
            entry.State = EntityState.Detached;
        }

        if (dbContext.Entry(currentRecord).State == EntityState.Detached)
        {
            dbContext.Attach(currentRecord);
            dbContext.Entry(currentRecord).State = currentRecord.IsCompleted
                ? EntityState.Modified
                : EntityState.Unchanged;
        }
    }
}
