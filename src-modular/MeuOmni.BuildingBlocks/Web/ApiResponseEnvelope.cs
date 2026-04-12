using System.Text.Json.Serialization;

namespace MeuOmni.BuildingBlocks.Web;

public sealed class ApiResponseEnvelope
{
    public required bool Success { get; init; }

    public required string Message { get; init; }

    public object? Data { get; init; }

    public IDictionary<string, object?>? Meta { get; init; }

    public string? ErrorCode { get; init; }

    public IReadOnlyCollection<ApiErrorItem>? Errors { get; init; }

    public required string TraceId { get; init; }
}

public sealed class ApiErrorItem
{
    public ApiErrorItem(string? field, string message)
    {
        Field = field;
        Message = message;
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Field { get; }

    public string Message { get; }
}
