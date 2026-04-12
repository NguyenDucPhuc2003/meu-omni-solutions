using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.Modules.Operations.Domain.Scaffold.Entities;

public sealed class Device : TenantAggregateRoot
{
    private Device()
    {
    }

    public Device(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class Printer : TenantAggregateRoot
{
    private Printer()
    {
    }

    public Printer(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class StoreSetting : TenantAggregateRoot
{
    private StoreSetting()
    {
    }

    public StoreSetting(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class OperationalJob : TenantAggregateRoot
{
    private OperationalJob()
    {
    }

    public OperationalJob(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}
