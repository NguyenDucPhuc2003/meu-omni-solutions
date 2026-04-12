using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;

namespace MeuOmni.BuildingBlocks.Web;

[ApiController]
public abstract class BaseApiController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : ControllerBase
{
    protected string TenantId => tenantContextAccessor.RequireTenantId();

    protected string? CurrentUserId => currentUserContextAccessor.UserId;

    protected IReadOnlyCollection<string> CurrentRoles => currentUserContextAccessor.Roles;

    protected IReadOnlyCollection<string> CurrentPermissions => currentUserContextAccessor.Permissions;

    protected bool HasRole(params string[] roles) => currentUserContextAccessor.HasAnyRole(roles);

    protected bool HasPermissions(params string[] permissions) => currentUserContextAccessor.HasAllPermissions(permissions);
}
