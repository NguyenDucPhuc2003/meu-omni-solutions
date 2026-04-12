namespace MeuOmni.BuildingBlocks.Security;

public interface ITenantContextAccessor
{
    string? TenantId { get; }

    string RequireTenantId();
}
