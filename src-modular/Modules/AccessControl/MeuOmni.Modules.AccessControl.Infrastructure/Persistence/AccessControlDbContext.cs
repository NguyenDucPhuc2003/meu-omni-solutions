using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.AccessControl.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.AccessControl.Infrastructure.Persistence;

public sealed class AccessControlDbContext(
    DbContextOptions<AccessControlDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTenantEntity(modelBuilder.Entity<User>(), "access_control_user");
        ConfigureTenantEntity(modelBuilder.Entity<Role>(), "access_control_role");
        ConfigureTenantEntity(modelBuilder.Entity<Permission>(), "access_control_permission");
        ConfigureTenantEntity(modelBuilder.Entity<LoginSession>(), "access_control_login_session");
        ApplyTenantQueryFilters(modelBuilder);
    }

    private static void ConfigureTenantEntity<TEntity>(EntityTypeBuilder<TEntity> entity, string tableName)
        where TEntity : TenantAggregateRoot
    {
        entity.ToTable(tableName);
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.CreatedAtUtc).IsRequired();
    }
}
