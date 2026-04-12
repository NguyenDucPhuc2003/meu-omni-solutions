using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;

namespace MeuOmni.BuildingBlocks.Idempotency;

public sealed class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContextAccessor tenantContextAccessor,
        IIdempotencyStoreResolver idempotencyStoreResolver)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequireIdempotencyAttribute>() is null)
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(RequestHeaderNames.IdempotencyKey, out var headerValues))
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                $"Header '{RequestHeaderNames.IdempotencyKey}' is required for this request.",
                "idempotency_key_required");
            return;
        }

        var idempotencyKey = headerValues.ToString().Trim();
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                $"Header '{RequestHeaderNames.IdempotencyKey}' must not be empty.",
                "idempotency_key_invalid");
            return;
        }

        var store = idempotencyStoreResolver.Resolve(context);
        if (store is null)
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "The current endpoint requires idempotency, but no backing store was registered for it.",
                "idempotency_store_unavailable");
            return;
        }

        var requestHash = await ComputeRequestHashAsync(context.Request);
        var reservation = await store.ReserveAsync(
            tenantContextAccessor.RequireTenantId(),
            context.Request.Method.ToUpperInvariant(),
            context.Request.Path.Value ?? string.Empty,
            idempotencyKey,
            requestHash,
            context.RequestAborted);

        switch (reservation.Status)
        {
            case IdempotencyReservationStatus.Replay:
                await ReplayStoredResponseAsync(context, reservation.Record);
                return;
            case IdempotencyReservationStatus.InProgress:
                await ApiResponseFactory.WriteErrorAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    reservation.Message ?? "The current request is already in progress.",
                    "idempotency_request_in_progress");
                return;
            case IdempotencyReservationStatus.Conflict:
                await ApiResponseFactory.WriteErrorAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    reservation.Message ?? "The current idempotency key conflicts with a previous request.",
                    "idempotency_key_conflict");
                return;
        }

        var originalResponseBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            buffer.Position = 0;
            var responseBody = await ReadStreamAsync(buffer, leaveOpen: true, context.RequestAborted);

            if (IsSuccessfulStatusCode(context.Response.StatusCode))
            {
                await store.CompleteAsync(
                    reservation.Record,
                    context.Response.StatusCode,
                    context.Response.ContentType,
                    responseBody,
                    context.RequestAborted);
            }
            else
            {
                await store.ReleaseAsync(reservation.Record, context.RequestAborted);
            }

            buffer.Position = 0;
            await buffer.CopyToAsync(originalResponseBody, context.RequestAborted);
        }
        catch
        {
            await store.ReleaseAsync(reservation.Record, CancellationToken.None);
            throw;
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private static bool IsSuccessfulStatusCode(int statusCode)
    {
        return statusCode is >= 200 and < 300;
    }

    private static async Task<string> ComputeRequestHashAsync(HttpRequest request)
    {
        request.EnableBuffering();
        var payload = await ReadStreamAsync(request.Body, leaveOpen: true, request.HttpContext.RequestAborted);
        request.Body.Position = 0;

        var canonical = string.Join(
            "\n",
            request.Method.ToUpperInvariant(),
            request.Path.Value ?? string.Empty,
            request.QueryString.Value ?? string.Empty,
            payload);

        var bytes = Encoding.UTF8.GetBytes(canonical);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private static async Task ReplayStoredResponseAsync(HttpContext context, IdempotencyRequestRecord record)
    {
        context.Response.StatusCode = record.ResponseStatusCode ?? StatusCodes.Status200OK;
        context.Response.ContentType = record.ResponseContentType ?? "application/json; charset=utf-8";
        context.Response.Headers["X-Idempotency-Replayed"] = "true";
        await context.Response.WriteAsync(record.ResponseBody ?? string.Empty, context.RequestAborted);
    }

    private static async Task<string> ReadStreamAsync(Stream stream, bool leaveOpen, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: leaveOpen);
        return await reader.ReadToEndAsync(cancellationToken);
    }
}
