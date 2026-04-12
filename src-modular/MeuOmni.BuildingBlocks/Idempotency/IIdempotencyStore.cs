using Microsoft.AspNetCore.Http;

namespace MeuOmni.BuildingBlocks.Idempotency;

public interface IIdempotencyStore
{
    Task<IdempotencyReservationResult> ReserveAsync(
        string tenantId,
        string requestMethod,
        string requestPath,
        string idempotencyKey,
        string requestHash,
        CancellationToken cancellationToken = default);

    Task CompleteAsync(
        IdempotencyRequestRecord record,
        int statusCode,
        string? contentType,
        string? responseBody,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(
        IdempotencyRequestRecord record,
        CancellationToken cancellationToken = default);
}

public interface IIdempotencyStoreResolver
{
    IIdempotencyStore? Resolve(HttpContext context);
}

public enum IdempotencyReservationStatus
{
    Reserved = 1,
    Replay = 2,
    InProgress = 3,
    Conflict = 4
}

public sealed class IdempotencyReservationResult(
    IdempotencyReservationStatus status,
    IdempotencyRequestRecord record,
    string? message = null)
{
    public IdempotencyReservationStatus Status { get; } = status;

    public IdempotencyRequestRecord Record { get; } = record;

    public string? Message { get; } = message;
}
