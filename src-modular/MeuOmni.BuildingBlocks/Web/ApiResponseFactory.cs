using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Querying;

namespace MeuOmni.BuildingBlocks.Web;

public static class ApiResponseFactory
{
    public static ApiResponseEnvelope Success(HttpContext context, object? value, int statusCode)
    {
        if (value is ApiResponseEnvelope envelope)
        {
            return envelope;
        }

        if (TryCreatePagedSuccess(context, value, statusCode, out var pagedEnvelope))
        {
            return pagedEnvelope;
        }

        return new ApiResponseEnvelope
        {
            Success = true,
            Message = GetSuccessMessage(statusCode),
            Data = value,
            TraceId = context.TraceIdentifier
        };
    }

    public static ApiResponseEnvelope Error(
        HttpContext context,
        int statusCode,
        string? message = null,
        string? errorCode = null,
        IReadOnlyCollection<ApiErrorItem>? errors = null)
    {
        return new ApiResponseEnvelope
        {
            Success = false,
            Message = message ?? GetErrorMessage(statusCode),
            ErrorCode = errorCode ?? GetErrorCode(statusCode),
            Errors = errors,
            TraceId = context.TraceIdentifier
        };
    }

    public static ApiResponseEnvelope Error(HttpContext context, int statusCode, object? value)
    {
        if (value is ApiResponseEnvelope envelope)
        {
            return envelope;
        }

        if (value is ValidationProblemDetails validationProblemDetails)
        {
            return Error(
                context,
                statusCode,
                validationProblemDetails.Title ?? validationProblemDetails.Detail ?? GetErrorMessage(statusCode),
                validationProblemDetails.Extensions.TryGetValue("error_code", out var validationErrorCode)
                    ? validationErrorCode?.ToString()
                    : GetErrorCode(statusCode),
                validationProblemDetails.Errors
                    .SelectMany(pair => pair.Value.Select(message => new ApiErrorItem(pair.Key, message)))
                    .ToArray());
        }

        if (value is ProblemDetails problemDetails)
        {
            return Error(
                context,
                statusCode,
                problemDetails.Title ?? problemDetails.Detail ?? GetErrorMessage(statusCode),
                problemDetails.Extensions.TryGetValue("error_code", out var problemErrorCode)
                    ? problemErrorCode?.ToString()
                    : GetErrorCode(statusCode));
        }

        if (value is SerializableError serializableError)
        {
            var errors = serializableError
                .SelectMany(pair =>
                {
                    var messages = pair.Value switch
                    {
                        string singleMessage => [singleMessage],
                        string[] stringMessages => stringMessages,
                        IEnumerable<string> enumerableMessages => enumerableMessages.ToArray(),
                        _ => [pair.Value?.ToString() ?? "Validation error"]
                    };

                    return messages.Select(message => new ApiErrorItem(pair.Key, message));
                })
                .ToArray();

            return Error(context, statusCode, "Validation error", "validation_error", errors);
        }

        if (value is string stringValue)
        {
            return Error(context, statusCode, stringValue);
        }

        return Error(context, statusCode);
    }

    public static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string message,
        string? errorCode = null,
        IReadOnlyCollection<ApiErrorItem>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(Error(context, statusCode, message, errorCode, errors));
    }

    private static bool TryCreatePagedSuccess(HttpContext context, object? value, int statusCode, out ApiResponseEnvelope envelope)
    {
        envelope = default!;

        if (value is null)
        {
            return false;
        }

        var valueType = value.GetType();
        if (!valueType.IsGenericType || valueType.GetGenericTypeDefinition() != typeof(PagedResult<>))
        {
            return false;
        }

        var items = valueType.GetProperty(nameof(PagedResult<object>.Items))?.GetValue(value);
        var page = valueType.GetProperty(nameof(PagedResult<object>.Page))?.GetValue(value);
        var pageSize = valueType.GetProperty(nameof(PagedResult<object>.PageSize))?.GetValue(value);
        var totalCount = valueType.GetProperty(nameof(PagedResult<object>.TotalCount))?.GetValue(value);
        var sorts = valueType.GetProperty(nameof(PagedResult<object>.Sorts))?.GetValue(value);
        var filters = valueType.GetProperty(nameof(PagedResult<object>.Filters))?.GetValue(value);

        envelope = new ApiResponseEnvelope
        {
            Success = true,
            Message = GetSuccessMessage(statusCode),
            Data = items,
            Meta = new Dictionary<string, object?>
            {
                ["page"] = page,
                ["page_size"] = pageSize,
                ["total"] = totalCount,
                ["sorts"] = sorts,
                ["filters"] = filters
            },
            TraceId = context.TraceIdentifier
        };

        return true;
    }

    private static string GetSuccessMessage(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status201Created => "Created",
            StatusCodes.Status202Accepted => "Accepted",
            StatusCodes.Status204NoContent => "No content",
            _ => "OK"
        };
    }

    private static string GetErrorMessage(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status422UnprocessableEntity => "Validation error",
            _ => "Request failed"
        };
    }

    private static string GetErrorCode(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "BAD_REQUEST",
            StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
            StatusCodes.Status403Forbidden => "FORBIDDEN",
            StatusCodes.Status404NotFound => "NOT_FOUND",
            StatusCodes.Status409Conflict => "CONFLICT",
            StatusCodes.Status422UnprocessableEntity => "VALIDATION_ERROR",
            _ => "REQUEST_FAILED"
        };
    }
}
