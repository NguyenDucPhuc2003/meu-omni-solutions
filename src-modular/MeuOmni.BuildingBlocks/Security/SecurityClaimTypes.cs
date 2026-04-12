using System.Security.Claims;

namespace MeuOmni.BuildingBlocks.Security;

public static class SecurityClaimTypes
{
    public static readonly string[] UserId =
    [
        ClaimTypes.NameIdentifier,
        "sub",
        "user_id"
    ];

    public static readonly string[] TenantId =
    [
        "tenant_id",
        "tenant",
        "tid"
    ];

    public static readonly string[] Role =
    [
        ClaimTypes.Role,
        "role",
        "roles"
    ];

    public static readonly string[] Permission =
    [
        "permission",
        "permissions"
    ];
}
