namespace MeuOmni.BuildingBlocks.Security;

public interface ICurrentUserContextAccessor
{
    string? UserId { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool HasAnyRole(IEnumerable<string> requiredRoles);

    bool HasAllPermissions(IEnumerable<string> requiredPermissions);
}
