namespace MeuOmni.BuildingBlocks.Domain;

/// <summary>
/// Base class for tenant-scoped entities (not aggregates)
/// </summary>
public abstract class TenantEntity : Entity, ITenantScoped
{
    public string TenantId { get; protected set; } = string.Empty;

    protected void InitializeTenant(string tenantId)
    {
        TenantId = string.IsNullOrWhiteSpace(tenantId)
            ? throw new DomainException("Tenant id is required.")
            : tenantId.Trim();
    }
}
