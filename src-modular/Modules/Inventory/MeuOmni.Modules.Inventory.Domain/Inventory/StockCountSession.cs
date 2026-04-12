using MeuOmni.BuildingBlocks.Domain;
using Sieve.Attributes;

namespace MeuOmni.Modules.Inventory.Domain.Inventory;

public enum StockCountSessionStatus
{
    Open = 1,
    Completed = 2
}

public sealed class StockCountSession : TenantAggregateRoot
{
    private StockCountSession()
    {
    }

    public StockCountSession(string tenantId, Guid warehouseId, string sessionName, string? note)
    {
        InitializeTenant(tenantId);
        WarehouseId = warehouseId;
        SessionName = string.IsNullOrWhiteSpace(sessionName)
            ? throw new DomainException("Session name is required.")
            : sessionName.Trim();
        Note = Normalize(note);
        Status = StockCountSessionStatus.Open;
    }

    [Sieve(CanFilter = true, CanSort = true)]
    public Guid WarehouseId { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public string SessionName { get; private set; } = string.Empty;

    public string? Note { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public StockCountSessionStatus Status { get; private set; }

    [Sieve(CanFilter = true, CanSort = true)]
    public DateTime? CompletedAtUtc { get; private set; }

    public void Complete(string? note)
    {
        if (Status == StockCountSessionStatus.Completed)
        {
            throw new DomainException("Stock count session is already completed.");
        }

        Note = Normalize(note) ?? Note;
        Status = StockCountSessionStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        Touch();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
