using Microsoft.AspNetCore.Http;

namespace MeuOmni.BuildingBlocks.Security;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TenantContextAccessor tenantContextAccessor)
    {
        if (!IsApiRequest(context))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(RequestHeaderNames.TenantId, out var values))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "tenant_id_required",
                message = $"Header '{RequestHeaderNames.TenantId}' is required."
            });
            return;
        }

        var tenantId = values.ToString().Trim();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "tenant_id_invalid",
                message = $"Header '{RequestHeaderNames.TenantId}' must not be empty."
            });
            return;
        }

        tenantContextAccessor.SetTenantId(tenantId);
        await next(context);
    }

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
