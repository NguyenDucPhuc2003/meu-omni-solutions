using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.BuildingBlocks.Web;
using MeuOmni.Modules.AccessControl.Application.Scaffolding;

namespace MeuOmni.Modules.AccessControl.Api.Controllers;

[Route("api/modules/access-control/auth")]
public sealed class AuthController(
    ITenantContextAccessor tenantContextAccessor,
    ICurrentUserContextAccessor currentUserContextAccessor) : BaseApiController(tenantContextAccessor, currentUserContextAccessor)
{

    [HttpPost("login")]
    public IActionResult Login([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Auth"),
            operation = "POST /auth/login",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }

    [HttpPost("refresh-token")]
    public IActionResult RefreshToken([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Auth"),
            operation = "POST /auth/refresh-token",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }

    [HttpPost("logout")]
    public IActionResult Logout([FromBody] object? request, CancellationToken cancellationToken)
    {
        return Accepted(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Auth"),
            operation = "POST /auth/logout",
            tenantId = TenantId,
            userId = CurrentUserId,
            request
        });    }

    [HttpGet("me")]
    public IActionResult GetMe(CancellationToken cancellationToken)
    {
        return Ok(new
        {
            scaffold = true,
            module = "AccessControl",
            resource = AccessControlScaffoldCatalog.GetResource("Auth"),
            path = "/auth/me",
            tenantId = TenantId,
            userId = CurrentUserId
        });    }
}
