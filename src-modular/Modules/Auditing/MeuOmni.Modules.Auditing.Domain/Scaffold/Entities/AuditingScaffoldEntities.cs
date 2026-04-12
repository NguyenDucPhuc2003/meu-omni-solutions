using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.Modules.Auditing.Domain.Scaffold.Entities;

public sealed class AuditLogEntry : TenantAggregateRoot
{
    private AuditLogEntry()
    {
    }

    public AuditLogEntry(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}
