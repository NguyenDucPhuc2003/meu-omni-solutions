using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Idempotency;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Cashbook.Domain.Cashbooks;
using CashbookAggregate = MeuOmni.Modules.Cashbook.Domain.Cashbooks.Cashbook;

namespace MeuOmni.Modules.Cashbook.Infrastructure.Persistence;

public sealed class CashbookDbContext(
    DbContextOptions<CashbookDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIdempotencyRequest(modelBuilder.Entity<IdempotencyRequestRecord>());
        ConfigureCashbook(modelBuilder.Entity<CashbookAggregate>());
        ConfigureCashTransaction(modelBuilder.Entity<CashTransaction>());
        ConfigureCashReconciliation(modelBuilder.Entity<CashReconciliation>());
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

    private static void ConfigureCashbook(EntityTypeBuilder<CashbookAggregate> entity)
    {
        ConfigureTenantEntity(entity, "cashbook_cashbook");
        entity.Property(x => x.Code).HasMaxLength(64).IsRequired();
        entity.Property(x => x.Name).HasMaxLength(256).IsRequired();
        entity.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
        entity.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        entity.Property(x => x.CurrentBalance).HasPrecision(18, 2);
    }

    private static void ConfigureCashTransaction(EntityTypeBuilder<CashTransaction> entity)
    {
        ConfigureTenantEntity(entity, "cashbook_cash_transaction");
        entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.SubType).HasMaxLength(64);
        entity.Property(x => x.PaymentMethod).HasMaxLength(32);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.CounterpartyType).HasMaxLength(32);
        entity.Property(x => x.SourceDocumentType).HasMaxLength(64);
        entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        entity.Property(x => x.Note).HasMaxLength(512);
        entity.Property(x => x.CancellationReason).HasMaxLength(512);
    }

    private static void ConfigureCashReconciliation(EntityTypeBuilder<CashReconciliation> entity)
    {
        ConfigureTenantEntity(entity, "cashbook_cash_reconciliation");
        entity.Property(x => x.SystemAmount).HasPrecision(18, 2);
        entity.Property(x => x.CountedAmount).HasPrecision(18, 2);
        entity.Property(x => x.Difference).HasPrecision(18, 2);
        entity.Property(x => x.DifferenceReason).HasMaxLength(512);
        entity.Property(x => x.ConfirmedBy).HasMaxLength(128);
    }

    private static void ConfigureIdempotencyRequest(EntityTypeBuilder<IdempotencyRequestRecord> entity)
    {
        entity.ToTable("cashbook_idempotency_request");
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
