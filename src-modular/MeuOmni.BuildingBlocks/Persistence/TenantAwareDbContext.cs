using Microsoft.EntityFrameworkCore;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Security;

namespace MeuOmni.BuildingBlocks.Persistence;

public abstract class TenantAwareDbContext(
    DbContextOptions options,
    ITenantContextAccessor tenantContextAccessor) : DbContext(options)
{
    protected string? CurrentTenantId => tenantContextAccessor.TenantId;

    protected void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.IsOwned())
            {
                continue;
            }

            var clrType = entityType.ClrType;
            if (!typeof(ITenantScoped).IsAssignableFrom(clrType))
            {
                continue;
            }

            var method = typeof(TenantAwareDbContext)
                .GetMethod(nameof(SetTenantQueryFilter), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(clrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    public override int SaveChanges()
    {
        VerifyTenantAssignments();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        VerifyTenantAssignments();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        VerifyTenantAssignments();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        VerifyTenantAssignments();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void VerifyTenantAssignments()
    {
        var currentTenantId = tenantContextAccessor.TenantId;

        foreach (var entry in ChangeTracker.Entries<ITenantScoped>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Entity.TenantId))
            {
                throw new InvalidOperationException("Tenant-scoped entity is missing TenantId.");
            }

            if (!string.IsNullOrWhiteSpace(currentTenantId)
                && !string.Equals(entry.Entity.TenantId, currentTenantId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Tenant mismatch detected. Entity tenant '{entry.Entity.TenantId}' does not match request tenant '{currentTenantId}'.");
            }
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(entity => CurrentTenantId == null || entity.TenantId == CurrentTenantId);
    }
}
