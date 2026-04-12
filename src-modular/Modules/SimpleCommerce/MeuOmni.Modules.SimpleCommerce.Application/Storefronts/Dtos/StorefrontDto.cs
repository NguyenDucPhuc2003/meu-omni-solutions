namespace MeuOmni.Modules.SimpleCommerce.Application.Storefronts.Dtos;

public sealed class StorefrontDto
{
    public Guid Id { get; init; }

    public string TenantId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;

    public string LinkedSalesChannel { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}
