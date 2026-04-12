using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Suppliers.Domain.Suppliers;

namespace MeuOmni.Modules.Suppliers.Infrastructure.Persistence;

public sealed class SuppliersDbContext(
    DbContextOptions<SuppliersDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureSupplier(modelBuilder.Entity<Supplier>());
        ConfigureDebtTransaction(modelBuilder.Entity<SupplierDebtTransaction>());
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

    private static void ConfigureSupplier(EntityTypeBuilder<Supplier> entity)
    {
        ConfigureTenantEntity(entity, "suppliers_supplier");
        entity.Property(x => x.Code).HasMaxLength(64);
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(32);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.DebtBalance).HasPrecision(18, 2);
    }

    private static void ConfigureDebtTransaction(EntityTypeBuilder<SupplierDebtTransaction> entity)
    {
        ConfigureTenantEntity(entity, "suppliers_supplier_debt_transaction");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.SourceDocumentType).HasMaxLength(64);
        entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
    }
}
