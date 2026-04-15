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
        ConfigureCustomerGroup(modelBuilder.Entity<CustomerGroup>());
        ConfigureCustomer(modelBuilder.Entity<Customer>());
        ConfigureDebtTransaction(modelBuilder.Entity<CustomerDebtTransaction>());
        ApplyTenantQueryFilters(modelBuilder);
    }

    private static void ConfigureCustomerGroup(EntityTypeBuilder<CustomerGroup> entity)
    {
        ConfigureTenantEntity(entity, "customers_customer_group");
        entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(512);
        entity.Property(x => x.IsActive).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
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
        entity.Property(x => x.StoreId);
        entity.Property(x => x.Code).HasMaxLength(64);
        entity.Property(x => x.GroupId);
        entity.Property(x => x.FullName).HasMaxLength(256).IsRequired();
        entity.Property(x => x.CustomerType).HasMaxLength(32).IsRequired();
        entity.Property(x => x.CompanyName).HasMaxLength(256);
        entity.Property(x => x.TaxCode).HasMaxLength(64);
        entity.Property(x => x.Phone).HasMaxLength(32);
        entity.Property(x => x.Email).HasMaxLength(256);
        entity.Property(x => x.Gender).HasMaxLength(16);
        entity.Property(x => x.Birthday);
        entity.Property(x => x.AddressLine).HasMaxLength(256);
        entity.Property(x => x.Ward).HasMaxLength(128);
        entity.Property(x => x.District).HasMaxLength(128);
        entity.Property(x => x.City).HasMaxLength(128);
        entity.Property(x => x.Note).HasMaxLength(512);
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
