using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MeuOmni.BuildingBlocks.Web;

namespace MeuOmni.BuildingBlocks.Security;

public sealed class TenantResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TenantContextAccessor tenantContextAccessor)
    {
        if (!IsApiRequest(context) || IsAnonymousEndpoint(context))
        {
            await next(context);
            return;
        }

        var tokenTenantId = ResolveTenantIdFromClaims(context.User);
        var hasOverrideHeader = TryGetTenantOverrideHeader(context, out var overrideTenantId);

        if (hasOverrideHeader && string.IsNullOrWhiteSpace(overrideTenantId))
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                $"Header '{RequestHeaderNames.TenantId}' must not be empty when used as a cross-tenant override.",
                "tenant_override_invalid");
            return;
        }

        if (string.IsNullOrWhiteSpace(tokenTenantId))
        {
            if (hasOverrideHeader && HasCrossTenantOverrideAccess(context.User))
            {
                tenantContextAccessor.SetTenantId(overrideTenantId!);
                await next(context);
                return;
            }

            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Authenticated token must contain a tenant_id claim.",
                "tenant_claim_required");
            return;
        }

        if (!hasOverrideHeader)
        {
            tenantContextAccessor.SetTenantId(tokenTenantId);
            await next(context);
            return;
        }

        if (string.Equals(tokenTenantId, overrideTenantId, StringComparison.OrdinalIgnoreCase))
        {
            tenantContextAccessor.SetTenantId(tokenTenantId);
            await next(context);
            return;
        }

        if (!HasCrossTenantOverrideAccess(context.User))
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status403Forbidden,
                $"Header '{RequestHeaderNames.TenantId}' is reserved for approved cross-tenant requests.",
                "tenant_override_forbidden");
            return;
        }

        tenantContextAccessor.SetTenantId(overrideTenantId!);
        await next(context);
    }

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAnonymousEndpoint(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;
    }

    private static string? ResolveTenantIdFromClaims(ClaimsPrincipal principal)
    {
        return SecurityClaimTypes.TenantId
            .Select(type => principal.FindFirst(type)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            ?.Trim();
    }

    private static bool TryGetTenantOverrideHeader(HttpContext context, out string? tenantId)
    {
        tenantId = null;

        if (!context.Request.Headers.TryGetValue(RequestHeaderNames.TenantId, out var values))
        {
            return false;
        }

        var candidate = values.ToString().Trim();
        if (string.IsNullOrWhiteSpace(candidate))
        {
            tenantId = null;
            return true;
        }

        tenantId = candidate;
        return true;
    }

    private static bool HasCrossTenantOverrideAccess(ClaimsPrincipal principal)
    {
        var roles = principal.Claims
            .Where(claim => SecurityClaimTypes.Role.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            .SelectMany(claim => ExpandClaimValue(claim.Value, splitOnSpaces: false))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (roles.Overlaps(SecurityAccessConventions.CrossTenantOverrideRoles))
        {
            return true;
        }

        var permissions = principal.Claims
            .Where(claim => SecurityClaimTypes.Permission.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            .SelectMany(claim => ExpandClaimValue(claim.Value, splitOnSpaces: true))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return permissions.Overlaps(SecurityAccessConventions.CrossTenantOverridePermissions);
    }

    private static IEnumerable<string> ExpandClaimValue(string value, bool splitOnSpaces)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var separators = splitOnSpaces
            ? new[] { ',', ';', ' ' }
            : new[] { ',', ';' };

        return value.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
