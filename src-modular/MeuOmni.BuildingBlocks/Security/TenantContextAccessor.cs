namespace MeuOmni.BuildingBlocks.Security;

public sealed class TenantContextAccessor : ITenantContextAccessor
{
    public string? TenantId { get; private set; }

    public string RequireTenantId()
    {
        return string.IsNullOrWhiteSpace(TenantId)
            ? throw new InvalidOperationException("Tenant context is not available for the current request.")
            : TenantId;
    }

    internal void SetTenantId(string tenantId)
    {
        TenantId = tenantId;
    }
}
