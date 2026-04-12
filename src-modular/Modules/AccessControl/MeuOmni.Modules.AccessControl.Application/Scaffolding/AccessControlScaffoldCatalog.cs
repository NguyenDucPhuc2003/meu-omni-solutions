using MeuOmni.BuildingBlocks.Scaffolding;

namespace MeuOmni.Modules.AccessControl.Application.Scaffolding;

public static class AccessControlScaffoldCatalog
{
    private static readonly IReadOnlyDictionary<string, ScaffoldResourceDescriptor> Resources =
        new Dictionary<string, ScaffoldResourceDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
        ["Auth"] = new ScaffoldResourceDescriptor(
            "Auth",
            "/auth",
            [
                new ScaffoldEndpointDescriptor("POST", "/auth/login", "Authenticate and issue token."),
                new ScaffoldEndpointDescriptor("POST", "/auth/refresh-token", "Refresh token."),
                new ScaffoldEndpointDescriptor("POST", "/auth/logout", "Logout current session."),
                new ScaffoldEndpointDescriptor("GET", "/auth/me", "Get current profile.")
            ]),
        ["Users"] = new ScaffoldResourceDescriptor(
            "Users",
            "/users",
            [
                new ScaffoldEndpointDescriptor("GET", "/users", "List users."),
                new ScaffoldEndpointDescriptor("POST", "/users", "Create user."),
                new ScaffoldEndpointDescriptor("GET", "/users/{id}", "Get user by id."),
                new ScaffoldEndpointDescriptor("PATCH", "/users/{id}", "Update user."),
                new ScaffoldEndpointDescriptor("POST", "/users/{id}/actions/activate", "Activate user."),
                new ScaffoldEndpointDescriptor("POST", "/users/{id}/actions/deactivate", "Deactivate user."),
                new ScaffoldEndpointDescriptor("POST", "/users/{id}/actions/reset-password", "Reset password.")
            ]),
        ["Roles"] = new ScaffoldResourceDescriptor(
            "Roles",
            "/roles",
            [
                new ScaffoldEndpointDescriptor("GET", "/roles", "List roles."),
                new ScaffoldEndpointDescriptor("POST", "/roles", "Create role."),
                new ScaffoldEndpointDescriptor("GET", "/roles/{id}", "Get role by id."),
                new ScaffoldEndpointDescriptor("PATCH", "/roles/{id}", "Update role.")
            ]),
        ["Permissions"] = new ScaffoldResourceDescriptor(
            "Permissions",
            "/permissions",
            [
                new ScaffoldEndpointDescriptor("GET", "/permissions", "List permissions.")
            ])
        };

    public static ScaffoldModuleDescriptor Describe()
    {
        return new ScaffoldModuleDescriptor("AccessControl", Resources.Values.ToArray());
    }

    public static ScaffoldResourceDescriptor GetResource(string resourceName)
    {
        return Resources[resourceName];
    }
}
