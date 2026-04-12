using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MeuOmni.BuildingBlocks.Security;

public sealed class EndpointAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ICurrentUserContextAccessor currentUserContextAccessor)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null || endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            await next(context);
            return;
        }

        var requiredRoles = endpoint.Metadata
            .GetOrderedMetadata<RequireRoleAttribute>()
            .SelectMany(x => x.Roles)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var requiredPermissions = endpoint.Metadata
            .GetOrderedMetadata<RequirePermissionAttribute>()
            .SelectMany(x => x.Permissions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (IsModuleApiRequest(context) && requiredRoles.Length == 0 && requiredPermissions.Length == 0)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "endpoint_security_metadata_missing",
                message = "Module API endpoints must declare at least one RequireRole or RequirePermission attribute."
            });
            return;
        }

        if (requiredRoles.Length == 0 && requiredPermissions.Length == 0)
        {
            await next(context);
            return;
        }

        if (!currentUserContextAccessor.IsAuthenticated)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "authentication_required",
                message = "An authenticated token or upstream authenticated principal is required for this endpoint."
            });
            return;
        }

        if (requiredRoles.Length > 0 && !currentUserContextAccessor.HasAnyRole(requiredRoles))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "role_forbidden",
                message = "Current user does not satisfy the required role.",
                required_roles = requiredRoles
            });
            return;
        }

        if (requiredPermissions.Length > 0 && !currentUserContextAccessor.HasAllPermissions(requiredPermissions))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "permission_forbidden",
                message = "Current user does not satisfy the required permission set.",
                required_permissions = requiredPermissions
            });
            return;
        }

        await next(context);
    }

    private static bool IsModuleApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api/modules", StringComparison.OrdinalIgnoreCase);
    }
}
