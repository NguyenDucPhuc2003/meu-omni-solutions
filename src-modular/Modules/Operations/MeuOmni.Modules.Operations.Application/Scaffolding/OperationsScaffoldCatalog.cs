using MeuOmni.BuildingBlocks.Scaffolding;

namespace MeuOmni.Modules.Operations.Application.Scaffolding;

public static class OperationsScaffoldCatalog
{
    private static readonly IReadOnlyDictionary<string, ScaffoldResourceDescriptor> Resources =
        new Dictionary<string, ScaffoldResourceDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
        ["Devices"] = new ScaffoldResourceDescriptor(
            "Devices",
            "/devices",
            [
                new ScaffoldEndpointDescriptor("GET", "/devices", "List devices."),
                new ScaffoldEndpointDescriptor("POST", "/devices", "Create device."),
                new ScaffoldEndpointDescriptor("GET", "/devices/{id}", "Get device by id."),
                new ScaffoldEndpointDescriptor("PATCH", "/devices/{id}", "Update device."),
                new ScaffoldEndpointDescriptor("POST", "/devices/{id}/actions/test", "Test device.")
            ]),
        ["Printers"] = new ScaffoldResourceDescriptor(
            "Printers",
            "/printers",
            [
                new ScaffoldEndpointDescriptor("GET", "/printers", "List printers."),
                new ScaffoldEndpointDescriptor("POST", "/printers", "Create printer."),
                new ScaffoldEndpointDescriptor("GET", "/printers/{id}", "Get printer by id."),
                new ScaffoldEndpointDescriptor("PATCH", "/printers/{id}", "Update printer."),
                new ScaffoldEndpointDescriptor("POST", "/printers/{id}/actions/test-print", "Test print.")
            ]),
        ["StoreSettings"] = new ScaffoldResourceDescriptor(
            "StoreSettings",
            "/store-settings",
            [
                new ScaffoldEndpointDescriptor("GET", "/store-settings", "Get store settings."),
                new ScaffoldEndpointDescriptor("PATCH", "/store-settings", "Update store settings.")
            ]),
        ["Operations"] = new ScaffoldResourceDescriptor(
            "Operations",
            "/operations",
            [
                new ScaffoldEndpointDescriptor("POST", "/operations/actions/backup", "Run backup job."),
                new ScaffoldEndpointDescriptor("POST", "/operations/actions/export", "Run export job.")
            ])
        };

    public static ScaffoldModuleDescriptor Describe()
    {
        return new ScaffoldModuleDescriptor("Operations", Resources.Values.ToArray());
    }

    public static ScaffoldResourceDescriptor GetResource(string resourceName)
    {
        return Resources[resourceName];
    }
}
