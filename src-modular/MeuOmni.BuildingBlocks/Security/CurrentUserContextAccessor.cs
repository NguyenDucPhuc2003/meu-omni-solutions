namespace MeuOmni.BuildingBlocks.Security;

public sealed class CurrentUserContextAccessor : ICurrentUserContextAccessor
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private readonly HashSet<string> roles = new(Comparer);
    private readonly HashSet<string> permissions = new(Comparer);

    public string? UserId { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);

    public IReadOnlyCollection<string> Roles => roles;

    public IReadOnlyCollection<string> Permissions => permissions;

    public bool HasAnyRole(IEnumerable<string> requiredRoles)
    {
        return requiredRoles.Any(roles.Contains);
    }

    public bool HasAllPermissions(IEnumerable<string> requiredPermissions)
    {
        return requiredPermissions.All(permissions.Contains);
    }

    internal void SetUserId(string? userId)
    {
        UserId = string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
    }

    internal void SetRoles(IEnumerable<string> values)
    {
        roles.Clear();
        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
        {
            roles.Add(value);
        }
    }

    internal void SetPermissions(IEnumerable<string> values)
    {
        permissions.Clear();
        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
        {
            permissions.Add(value);
        }
    }
}
