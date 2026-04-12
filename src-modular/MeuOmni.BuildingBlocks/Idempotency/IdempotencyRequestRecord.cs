using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.BuildingBlocks.Idempotency;

public sealed class IdempotencyRequestRecord : ITenantScoped
{
    private IdempotencyRequestRecord()
    {
    }

    private IdempotencyRequestRecord(
        string tenantId,
        string requestMethod,
        string requestPath,
        string idempotencyKey,
        string requestHash)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new DomainException("Tenant id is required.");
        }

        if (string.IsNullOrWhiteSpace(requestMethod))
        {
            throw new DomainException("Request method is required.");
        }

        if (string.IsNullOrWhiteSpace(requestPath))
        {
            throw new DomainException("Request path is required.");
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new DomainException("Idempotency key is required.");
        }

        if (string.IsNullOrWhiteSpace(requestHash))
        {
            throw new DomainException("Request hash is required.");
        }

        Id = Guid.NewGuid();
        TenantId = tenantId.Trim();
        RequestMethod = requestMethod.Trim().ToUpperInvariant();
        RequestPath = requestPath.Trim();
        IdempotencyKey = idempotencyKey.Trim();
        RequestHash = requestHash.Trim();
        State = IdempotencyRequestStates.Pending;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }

    public string TenantId { get; private set; } = string.Empty;

    public string RequestMethod { get; private set; } = string.Empty;

    public string RequestPath { get; private set; } = string.Empty;

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string RequestHash { get; private set; } = string.Empty;

    public string State { get; private set; } = IdempotencyRequestStates.Pending;

    public int? ResponseStatusCode { get; private set; }

    public string? ResponseContentType { get; private set; }

    public string? ResponseBody { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public static IdempotencyRequestRecord Reserve(
        string tenantId,
        string requestMethod,
        string requestPath,
        string idempotencyKey,
        string requestHash)
    {
        return new IdempotencyRequestRecord(tenantId, requestMethod, requestPath, idempotencyKey, requestHash);
    }

    public bool IsCompleted => string.Equals(State, IdempotencyRequestStates.Completed, StringComparison.OrdinalIgnoreCase);

    public bool IsPending => string.Equals(State, IdempotencyRequestStates.Pending, StringComparison.OrdinalIgnoreCase);

    public bool IsExpired(TimeSpan timeout, DateTime utcNow)
    {
        return IsPending && CreatedAtUtc.Add(timeout) <= utcNow;
    }

    public void Complete(int statusCode, string? contentType, string? responseBody)
    {
        ResponseStatusCode = statusCode;
        ResponseContentType = string.IsNullOrWhiteSpace(contentType) ? "application/json; charset=utf-8" : contentType.Trim();
        ResponseBody = responseBody ?? string.Empty;
        State = IdempotencyRequestStates.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = CompletedAtUtc.Value;
    }
}

public static class IdempotencyRequestStates
{
    public const string Pending = "PENDING";
    public const string Completed = "COMPLETED";
}
