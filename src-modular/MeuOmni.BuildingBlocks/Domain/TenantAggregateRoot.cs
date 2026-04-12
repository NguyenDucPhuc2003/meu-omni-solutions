namespace MeuOmni.BuildingBlocks.Domain;

public abstract class TenantAggregateRoot : AggregateRoot, ITenantScoped
{
    public string TenantId { get; private set; } = string.Empty;

    protected void InitializeTenant(string tenantId)
    {
        TenantId = string.IsNullOrWhiteSpace(tenantId)
            ? throw new DomainException("Tenant id is required.")
            : tenantId.Trim();
    }
}
