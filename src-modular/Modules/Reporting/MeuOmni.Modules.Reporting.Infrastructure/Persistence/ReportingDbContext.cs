using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MeuOmni.BuildingBlocks.Domain;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Reporting.Domain.Scaffold.Entities;

namespace MeuOmni.Modules.Reporting.Infrastructure.Persistence;

public sealed class ReportingDbContext(
    DbContextOptions<ReportingDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureTenantEntity(modelBuilder.Entity<SalesDashboardReadModel>(), "reporting_sales_dashboard_read_model");
        ConfigureTenantEntity(modelBuilder.Entity<ShiftSummaryReadModel>(), "reporting_shift_summary_read_model");
        ConfigureTenantEntity(modelBuilder.Entity<SalesReportReadModel>(), "reporting_sales_report_read_model");
        ConfigureTenantEntity(modelBuilder.Entity<InventorySummaryReadModel>(), "reporting_inventory_summary_read_model");
        ConfigureTenantEntity(modelBuilder.Entity<CashFlowReadModel>(), "reporting_cash_flow_read_model");
        ConfigureTenantEntity(modelBuilder.Entity<CustomerDebtReportReadModel>(), "reporting_customer_debt_report_read_model");
        ConfigureTenantEntity(modelBuilder.Entity<SupplierDebtReportReadModel>(), "reporting_supplier_debt_report_read_model");
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
