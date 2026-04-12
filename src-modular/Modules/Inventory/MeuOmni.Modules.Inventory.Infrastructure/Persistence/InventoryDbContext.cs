using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Idempotency;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Inventory.Domain.Inventory;

namespace MeuOmni.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(
    DbContextOptions<InventoryDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIdempotencyRequest(modelBuilder.Entity<IdempotencyRequestRecord>());
        ConfigureStockTransaction(modelBuilder.Entity<StockTransaction>());
        ConfigureStockLevel(modelBuilder.Entity<StockLevel>());
        ConfigureStockCountSession(modelBuilder.Entity<StockCountSession>());
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

    private static void ConfigureStockTransaction(EntityTypeBuilder<StockTransaction> entity)
    {
        ConfigureTenantEntity(entity, "inventory_stock_transaction");
        entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.Note).HasMaxLength(512);
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.CancellationReason).HasMaxLength(512);

        entity.OwnsMany(x => x.Items, item =>
        {
            item.ToTable("inventory_stock_transaction_item");
            item.WithOwner().HasForeignKey("StockTransactionId");
            item.HasKey(x => x.Id);
            item.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            item.Property(x => x.Quantity).HasPrecision(18, 2);
            item.Property(x => x.UnitCost).HasPrecision(18, 2);
        });

        entity.Navigation(x => x.Items).AutoInclude();
    }

    private static void ConfigureStockLevel(EntityTypeBuilder<StockLevel> entity)
    {
        ConfigureTenantEntity(entity, "inventory_stock_level");
        entity.Property(x => x.OnHandQuantity).HasPrecision(18, 2);
    }

    private static void ConfigureStockCountSession(EntityTypeBuilder<StockCountSession> entity)
    {
        ConfigureTenantEntity(entity, "inventory_stock_count_session");
        entity.Property(x => x.SessionName).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Note).HasMaxLength(512);
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
    }

    private static void ConfigureIdempotencyRequest(EntityTypeBuilder<IdempotencyRequestRecord> entity)
    {
        entity.ToTable("inventory_idempotency_request");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        entity.Property(x => x.RequestMethod).HasMaxLength(16).IsRequired();
        entity.Property(x => x.RequestPath).HasMaxLength(256).IsRequired();
        entity.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
        entity.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.State).HasMaxLength(32).IsRequired();
        entity.Property(x => x.ResponseContentType).HasMaxLength(128);
        entity.Property(x => x.CreatedAtUtc).IsRequired();
        entity.Property(x => x.UpdatedAtUtc).IsRequired();
        entity.HasIndex(x => new { x.TenantId, x.RequestMethod, x.RequestPath, x.IdempotencyKey })
            .IsUnique();
    }
}
