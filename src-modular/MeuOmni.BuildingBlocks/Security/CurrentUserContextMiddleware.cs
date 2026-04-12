using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace MeuOmni.BuildingBlocks.Security;

public sealed class CurrentUserContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, CurrentUserContextAccessor currentUserContextAccessor)
    {
        if (!IsApiRequest(context))
        {
            await next(context);
            return;
        }

        currentUserContextAccessor.SetUserId(GetCurrentUserId(context));
        currentUserContextAccessor.SetRoles(GetRoleClaims(context.User));
        currentUserContextAccessor.SetPermissions(GetPermissionClaims(context.User));

        await next(context);
    }

    private static string? GetCurrentUserId(HttpContext context)
    {
        var claimValue = SecurityClaimTypes.UserId
            .Select(type => context.User.FindFirst(type)?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            return claimValue;
        }

        return context.Request.Headers.TryGetValue(RequestHeaderNames.UserId, out var values)
            ? values.ToString()
            : null;
    }

    private static IEnumerable<string> GetRoleClaims(ClaimsPrincipal principal)
    {
        return principal.Claims
            .Where(claim => SecurityClaimTypes.Role.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            .SelectMany(claim => ExpandClaimValue(claim.Value, splitOnSpaces: false))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> GetPermissionClaims(ClaimsPrincipal principal)
    {
        return principal.Claims
            .Where(claim => SecurityClaimTypes.Permission.Contains(claim.Type, StringComparer.OrdinalIgnoreCase))
            .SelectMany(claim => ExpandClaimValue(claim.Value, splitOnSpaces: true))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ExpandClaimValue(string value, bool splitOnSpaces)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        var trimmed = value.Trim();
        if (TryParseJsonArray(trimmed, out var jsonValues))
        {
            return jsonValues;
        }

        var separators = splitOnSpaces
            ? new[] { ',', ';', ' ' }
            : new[] { ',', ';' };

        return trimmed.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool TryParseJsonArray(string value, out IEnumerable<string> items)
    {
        items = [];

        if (!value.StartsWith('[') || !value.EndsWith(']'))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            items = document.RootElement
                .EnumerateArray()
                .Where(element => element.ValueKind == JsonValueKind.String)
                .Select(element => element.GetString())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item!.Trim())
                .ToArray();

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
    }
}
