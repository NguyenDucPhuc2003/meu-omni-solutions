using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Catalog.Domain.Catalog;

namespace MeuOmni.Modules.Catalog.Infrastructure.Persistence;

public sealed class CatalogDbContext(
    DbContextOptions<CatalogDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureProduct(modelBuilder.Entity<Product>());
        ConfigureProductPrice(modelBuilder.Entity<ProductPrice>());
        ConfigureCategory(modelBuilder.Entity<Category>());
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

    private static void ConfigureProduct(EntityTypeBuilder<Product> entity)
    {
        ConfigureTenantEntity(entity, "catalog_product");
        entity.Property(x => x.Code).HasMaxLength(64);
        entity.Property(x => x.Sku).HasMaxLength(64);
        entity.Property(x => x.Barcode).HasMaxLength(64);
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
        entity.Property(x => x.SellPrice).HasPrecision(18, 2);
    }

    private static void ConfigureProductPrice(EntityTypeBuilder<ProductPrice> entity)
    {
        ConfigureTenantEntity(entity, "catalog_product_price");
        entity.Property(x => x.PriceType).HasMaxLength(32).IsRequired();
        entity.Property(x => x.Price).HasPrecision(18, 2);
    }

    private static void ConfigureCategory(EntityTypeBuilder<Category> entity)
    {
        ConfigureTenantEntity(entity, "catalog_category");
        entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
    }
}
