namespace MeuOmni.BuildingBlocks.Security;

public static class SecurityAccessConventions
{
    public static readonly string[] CrossTenantOverrideRoles =
    [
        "super-admin",
        "platform-admin",
        "platform_admin"
    ];

    public static readonly string[] CrossTenantOverridePermissions =
    [
        "platform.tenants.assume",
        "platform.tenants.override"
    ];
}
