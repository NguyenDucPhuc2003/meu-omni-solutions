using MeuOmni.BuildingBlocks.Querying;

namespace MeuOmni.Modules.Inventory.Application.Inventory;

public sealed class StockTransactionListQuery : MeuOmniSieveModel;
public sealed class StockLevelListQuery : MeuOmniSieveModel;
public sealed class StockCountSessionListQuery : MeuOmniSieveModel;

public sealed class CreateStockTransactionRequest
{
    public string? TenantId { get; init; }
    public required string Type { get; init; }
    public Guid? WarehouseId { get; init; }
    public Guid? FromWarehouseId { get; init; }
    public Guid? ToWarehouseId { get; init; }
    public string? Note { get; init; }
    public List<CreateStockTransactionItemRequest> Items { get; init; } = [];
}

public sealed class CreateStockTransactionItemRequest
{
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitCost { get; init; }
}

public sealed class CancelStockTransactionRequest
{
    public string? Reason { get; init; }
}

public sealed class CreateStockCountSessionRequest
{
    public string? TenantId { get; init; }
    public Guid WarehouseId { get; init; }
    public required string SessionName { get; init; }
    public string? Note { get; init; }
}

public sealed class CompleteStockCountSessionRequest
{
    public string? Note { get; init; }
}

public sealed class StockTransactionItemDto
{
    public Guid ProductId { get; init; }
    public decimal Quantity { get; init; }
    public decimal? UnitCost { get; init; }
}

public sealed class StockTransactionDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public Guid? WarehouseId { get; init; }
    public Guid? FromWarehouseId { get; init; }
    public Guid? ToWarehouseId { get; init; }
    public string? Note { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyCollection<StockTransactionItemDto> Items { get; init; } = [];
}

public sealed class StockLevelDto
{
    public Guid WarehouseId { get; init; }
    public Guid ProductId { get; init; }
    public decimal OnHandQuantity { get; init; }
}

public sealed class StockCountSessionDto
{
    public Guid Id { get; init; }
    public Guid WarehouseId { get; init; }
    public string SessionName { get; init; } = string.Empty;
    public string? Note { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime? CompletedAtUtc { get; init; }
}

public interface IStockTransactionApplicationService
{
    Task<PagedResult<StockTransactionDto>> ListAsync(StockTransactionListQuery query, CancellationToken cancellationToken = default);
    Task<StockTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<StockTransactionDto> CreateAsync(CreateStockTransactionRequest request, CancellationToken cancellationToken = default);
    Task<StockTransactionDto?> CancelAsync(Guid transactionId, CancelStockTransactionRequest request, CancellationToken cancellationToken = default);
}

public interface IStockLevelApplicationService
{
    Task<PagedResult<StockLevelDto>> ListAsync(StockLevelListQuery query, CancellationToken cancellationToken = default);
    Task<StockLevelDto?> GetByWarehouseProductAsync(Guid warehouseId, Guid productId, CancellationToken cancellationToken = default);
}

public interface IStockCountSessionApplicationService
{
    Task<PagedResult<StockCountSessionDto>> ListAsync(StockCountSessionListQuery query, CancellationToken cancellationToken = default);
    Task<StockCountSessionDto?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<StockCountSessionDto> CreateAsync(CreateStockCountSessionRequest request, CancellationToken cancellationToken = default);
    Task<StockCountSessionDto?> CompleteAsync(Guid sessionId, CompleteStockCountSessionRequest request, CancellationToken cancellationToken = default);
}
