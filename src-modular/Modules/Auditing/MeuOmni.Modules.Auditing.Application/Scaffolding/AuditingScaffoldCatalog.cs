using MeuOmni.BuildingBlocks.Scaffolding;

namespace MeuOmni.Modules.Auditing.Application.Scaffolding;

public static class AuditingScaffoldCatalog
{
    private static readonly IReadOnlyDictionary<string, ScaffoldResourceDescriptor> Resources =
        new Dictionary<string, ScaffoldResourceDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
        ["AuditLogs"] = new ScaffoldResourceDescriptor(
            "AuditLogs",
            "/audit-logs",
            [
                new ScaffoldEndpointDescriptor("GET", "/audit-logs", "List audit logs."),
                new ScaffoldEndpointDescriptor("GET", "/audit-logs/{id}", "Get audit log by id.")
            ])
        };

    public static ScaffoldModuleDescriptor Describe()
    {
        return new ScaffoldModuleDescriptor("Auditing", Resources.Values.ToArray());
    }

    public static ScaffoldResourceDescriptor GetResource(string resourceName)
    {
        return Resources[resourceName];
    }
}
