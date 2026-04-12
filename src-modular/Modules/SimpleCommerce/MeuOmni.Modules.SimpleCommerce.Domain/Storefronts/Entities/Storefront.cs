using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Entities;

public sealed class Storefront : TenantAggregateRoot
{
    private Storefront()
    {
    }

    public Storefront(string tenantId, string name, string baseUrl, string linkedSalesChannel)
    {
        InitializeTenant(tenantId);
        Name = Require(name, "Storefront name");
        BaseUrl = Require(baseUrl, "Base URL");
        LinkedSalesChannel = Require(linkedSalesChannel, "Linked sales channel");
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string BaseUrl { get; private set; } = string.Empty;

    public string LinkedSalesChannel { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    private static string Require(string value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new DomainException($"{fieldName} is required.")
            : value.Trim();
    }
}
