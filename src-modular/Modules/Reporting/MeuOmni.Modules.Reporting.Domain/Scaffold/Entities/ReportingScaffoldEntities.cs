using MeuOmni.BuildingBlocks.Domain;

namespace MeuOmni.Modules.Reporting.Domain.Scaffold.Entities;

public sealed class SalesDashboardReadModel : TenantAggregateRoot
{
    private SalesDashboardReadModel()
    {
    }

    public SalesDashboardReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class ShiftSummaryReadModel : TenantAggregateRoot
{
    private ShiftSummaryReadModel()
    {
    }

    public ShiftSummaryReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class SalesReportReadModel : TenantAggregateRoot
{
    private SalesReportReadModel()
    {
    }

    public SalesReportReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class InventorySummaryReadModel : TenantAggregateRoot
{
    private InventorySummaryReadModel()
    {
    }

    public InventorySummaryReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class CashFlowReadModel : TenantAggregateRoot
{
    private CashFlowReadModel()
    {
    }

    public CashFlowReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class CustomerDebtReportReadModel : TenantAggregateRoot
{
    private CustomerDebtReportReadModel()
    {
    }

    public CustomerDebtReportReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}

public sealed class SupplierDebtReportReadModel : TenantAggregateRoot
{
    private SupplierDebtReportReadModel()
    {
    }

    public SupplierDebtReportReadModel(string tenantId)
    {
        InitializeTenant(tenantId);
    }
}
