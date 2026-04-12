using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.AccessControl.Domain.Scaffold.Entities;
using MeuOmni.Modules.AccessControl.Domain.Scaffold.Repositories;
using MeuOmni.Modules.AccessControl.Infrastructure.Persistence;

namespace MeuOmni.Modules.AccessControl.Infrastructure.Repositories;

public sealed class UserRepository(AccessControlDbContext dbContext) : IUserRepository
{
    public async Task AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<User>().AddAsync(entity, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<User>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<User>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<User>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class RoleRepository(AccessControlDbContext dbContext) : IRoleRepository
{
    public async Task AddAsync(Role entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Role>().AddAsync(entity, cancellationToken);
    }

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Role>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<Role>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Role>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class PermissionRepository(AccessControlDbContext dbContext) : IPermissionRepository
{
    public async Task AddAsync(Permission entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Permission>().AddAsync(entity, cancellationToken);
    }

    public Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Permission>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<Permission>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Permission>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class LoginSessionRepository(AccessControlDbContext dbContext) : ILoginSessionRepository
{
    public async Task AddAsync(LoginSession entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<LoginSession>().AddAsync(entity, cancellationToken);
    }

    public Task<LoginSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<LoginSession>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task<List<LoginSession>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<LoginSession>().OrderByDescending(entity => entity.CreatedAtUtc).ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
