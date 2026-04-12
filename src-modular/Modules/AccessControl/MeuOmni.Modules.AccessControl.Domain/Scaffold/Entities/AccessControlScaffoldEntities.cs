using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.Modules.AccessControl.Domain.Scaffold.Entities;

public sealed class User : TenantAggregateRoot
{
    private User()
    {
    }

    public User(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class Role : TenantAggregateRoot
{
    private Role()
    {
    }

    public Role(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class Permission : TenantAggregateRoot
{
    private Permission()
    {
    }

    public Permission(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class LoginSession : TenantAggregateRoot
{
    private LoginSession()
    {
    }

    public LoginSession(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}
