using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Idempotency;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.SalesChannel.Domain.Orders.Entities;

namespace MeuOmni.Modules.SalesChannel.Infrastructure.Persistence;

public sealed class SalesChannelDbContext(
    DbContextOptions<SalesChannelDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<SalesOrder>();
        ConfigureIdempotencyRequest(modelBuilder.Entity<IdempotencyRequestRecord>());
        order.ToTable("sales_orders");
        order.HasKey(x => x.Id);
        order.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        order.Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
        order.Property(x => x.SourceOrderNumber).HasMaxLength(100);
        order.Property(x => x.TotalAmount).HasPrecision(18, 2);
        order.Property(x => x.Channel).HasConversion<string>().HasMaxLength(32);
        order.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

        order.OwnsMany(x => x.Lines, line =>
        {
            line.ToTable("sales_order_lines");
            line.WithOwner().HasForeignKey("SalesOrderId");
            line.HasKey(x => x.Id);
            line.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
            line.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            line.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            line.Property(x => x.Quantity).HasPrecision(18, 2);
            line.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        order.Navigation(x => x.Lines).AutoInclude();

        ApplyTenantQueryFilters(modelBuilder);
    }

    private static void ConfigureIdempotencyRequest(EntityTypeBuilder<IdempotencyRequestRecord> entity)
    {
        entity.ToTable("sales_channel_idempotency_request");
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
