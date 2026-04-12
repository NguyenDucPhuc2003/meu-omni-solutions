using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Customers.Domain.Customers;

namespace MeuOmni.Modules.Customers.Infrastructure.Persistence;

public sealed class CustomersDbContext(
    DbContextOptions<CustomersDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureCustomer(modelBuilder.Entity<Customer>());
        ConfigureDebtTransaction(modelBuilder.Entity<CustomerDebtTransaction>());
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

    private static void ConfigureCustomer(EntityTypeBuilder<Customer> entity)
    {
        ConfigureTenantEntity(entity, "customers_customer");
        entity.Property(x => x.Code).HasMaxLength(64);
        entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Phone).HasMaxLength(32);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.DebtBalance).HasPrecision(18, 2);
        entity.Property(x => x.TotalSpent).HasPrecision(18, 2);
    }

    private static void ConfigureDebtTransaction(EntityTypeBuilder<CustomerDebtTransaction> entity)
    {
        ConfigureTenantEntity(entity, "customers_customer_debt_transaction");
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.SourceDocumentType).HasMaxLength(64);
        entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
    }
}
