using MeuOmni.BuildingBlocks.Scaffolding;

namespace MeuOmni.Modules.Reporting.Application.Scaffolding;

public static class ReportingScaffoldCatalog
{
    private static readonly IReadOnlyDictionary<string, ScaffoldResourceDescriptor> Resources =
        new Dictionary<string, ScaffoldResourceDescriptor>(StringComparer.OrdinalIgnoreCase)
        {
        ["Dashboard"] = new ScaffoldResourceDescriptor(
            "Dashboard",
            "/reports/dashboard",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/dashboard", "Get dashboard report.")
            ]),
        ["Sales"] = new ScaffoldResourceDescriptor(
            "Sales",
            "/reports/sales",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/sales", "Get sales report.")
            ]),
        ["Shifts"] = new ScaffoldResourceDescriptor(
            "Shifts",
            "/reports/shifts",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/shifts", "Get shift report.")
            ]),
        ["Inventory"] = new ScaffoldResourceDescriptor(
            "Inventory",
            "/reports/inventory",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/inventory", "Get inventory report.")
            ]),
        ["Cashflow"] = new ScaffoldResourceDescriptor(
            "Cashflow",
            "/reports/cashflow",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/cashflow", "Get cashflow report.")
            ]),
        ["CustomerDebt"] = new ScaffoldResourceDescriptor(
            "CustomerDebt",
            "/reports/customer-debt",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/customer-debt", "Get customer debt report.")
            ]),
        ["SupplierDebt"] = new ScaffoldResourceDescriptor(
            "SupplierDebt",
            "/reports/supplier-debt",
            [
                new ScaffoldEndpointDescriptor("GET", "/reports/supplier-debt", "Get supplier debt report.")
            ])
        };

    public static ScaffoldModuleDescriptor Describe()
    {
        return new ScaffoldModuleDescriptor("Reporting", Resources.Values.ToArray());
    }

    public static ScaffoldResourceDescriptor GetResource(string resourceName)
    {
        return Resources[resourceName];
    }
}
