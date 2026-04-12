using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Operations.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.Operations.Infrastructure.Persistence;

public sealed class OperationsDbContext(
    DbContextOptions<OperationsDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTenantEntity(modelBuilder.Entity<Device>(), "operations_device");
        ConfigureTenantEntity(modelBuilder.Entity<Printer>(), "operations_printer");
        ConfigureTenantEntity(modelBuilder.Entity<StoreSetting>(), "operations_store_setting");
        ConfigureTenantEntity(modelBuilder.Entity<OperationalJob>(), "operations_operational_job");
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
