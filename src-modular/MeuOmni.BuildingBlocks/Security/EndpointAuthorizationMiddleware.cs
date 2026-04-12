using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MeuOmni.BuildingBlocks.Web;

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
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Module API endpoints must declare at least one RequireRole or RequirePermission attribute.",
                "endpoint_security_metadata_missing");
            return;
        }

        if (requiredRoles.Length == 0 && requiredPermissions.Length == 0)
        {
            await next(context);
            return;
        }

        if (!currentUserContextAccessor.IsAuthenticated)
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "An authenticated token or upstream authenticated principal is required for this endpoint.",
                "authentication_required");
            return;
        }

        if (requiredRoles.Length > 0 && !currentUserContextAccessor.HasAnyRole(requiredRoles))
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Current user does not satisfy the required role.",
                "role_forbidden",
                [new ApiErrorItem("required_roles", string.Join(", ", requiredRoles))]);
            return;
        }

        if (requiredPermissions.Length > 0 && !currentUserContextAccessor.HasAllPermissions(requiredPermissions))
        {
            await ApiResponseFactory.WriteErrorAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Current user does not satisfy the required permission set.",
                "permission_forbidden",
                [new ApiErrorItem("required_permissions", string.Join(", ", requiredPermissions))]);
            return;
        }

        await next(context);
    }

    private static bool IsModuleApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api/modules", StringComparison.OrdinalIgnoreCase);
    }
}
