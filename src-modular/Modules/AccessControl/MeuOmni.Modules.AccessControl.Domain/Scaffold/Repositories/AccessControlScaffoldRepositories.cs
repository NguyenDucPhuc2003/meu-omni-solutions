using MeuOmni.Modules.AccessControl.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.AccessControl.Domain.Scaffold.Repositories;

public interface IUserRepository
{
    Task AddAsync(User entity, CancellationToken cancellationToken = default);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<User>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IRoleRepository
{
    Task AddAsync(Role entity, CancellationToken cancellationToken = default);

    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Role>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IPermissionRepository
{
    Task AddAsync(Permission entity, CancellationToken cancellationToken = default);

    Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Permission>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface ILoginSessionRepository
{
    Task AddAsync(LoginSession entity, CancellationToken cancellationToken = default);

    Task<LoginSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<LoginSession>> ListAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
