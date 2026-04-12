namespace MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Commands;

public sealed class CreateStorefrontRequest
{
    public string TenantId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public string LinkedSalesChannel { get; init; } = "Website";
}
