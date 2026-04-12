namespace MeuOmni.BuildingBlocks.Security;

public static class TenantContextGuard
{
    public static string ResolveTenantId(ITenantContextAccessor tenantContextAccessor, string? requestTenantId = null)
    {
        var currentTenantId = tenantContextAccessor.RequireTenantId();

        if (!string.IsNullOrWhiteSpace(requestTenantId)
            && !string.Equals(requestTenantId.Trim(), currentTenantId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Tenant mismatch detected. Request tenant '{requestTenantId}' does not match current tenant '{currentTenantId}'.");
        }

        return currentTenantId;
    }
}
