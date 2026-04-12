using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Inventory.Domain.Inventory;
using Sieve.Services;

namespace MeuOmni.Modules.Inventory.Application.Inventory;

public sealed class StockTransactionApplicationService(
    IStockTransactionRepository stockTransactionRepository,
    IStockLevelRepository stockLevelRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : IStockTransactionApplicationService
{
    public Task<PagedResult<StockTransactionDto>> ListAsync(StockTransactionListQuery query, CancellationToken cancellationToken = default)
    {
        return stockTransactionRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<StockTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await stockTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        return transaction is null ? null : ToDto(transaction);
    }

    public async Task<StockTransactionDto> CreateAsync(CreateStockTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);

        if (!Enum.TryParse<StockTransactionType>(request.Type, true, out var transactionType))
        {
            throw new InvalidOperationException("Stock transaction type is invalid.");
        }

        if (request.Items.Count == 0)
        {
            throw new InvalidOperationException("Stock transaction requires at least one item.");
        }

        var transaction = new StockTransaction(
            tenantId,
            transactionType,
            request.WarehouseId,
            request.FromWarehouseId,
            request.ToWarehouseId,
            request.Note);

        foreach (var item in request.Items)
        {
            transaction.AddItem(item.ProductId, item.Quantity, item.UnitCost);
        }

        await stockTransactionRepository.AddAsync(transaction, cancellationToken);
        await ApplyToStockLevelsAsync(tenantId, transaction, reverse: false, cancellationToken);
        await stockTransactionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    public async Task<StockTransactionDto?> CancelAsync(Guid transactionId, CancelStockTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await stockTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        transaction.Cancel(request.Reason);
        await ApplyToStockLevelsAsync(transaction.TenantId, transaction, reverse: true, cancellationToken);
        await stockTransactionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    private async Task ApplyToStockLevelsAsync(
        string tenantId,
        StockTransaction transaction,
        bool reverse,
        CancellationToken cancellationToken)
    {
        foreach (var item in transaction.Items)
        {
            var multiplier = reverse ? -1m : 1m;
            switch (transaction.Type)
            {
                case StockTransactionType.PurchaseIn:
                case StockTransactionType.ReturnIn:
                case StockTransactionType.AdjustIn:
                    await AdjustLevelAsync(tenantId, transaction.WarehouseId!.Value, item.ProductId, item.Quantity * multiplier, cancellationToken);
                    break;
                case StockTransactionType.SaleOut:
                case StockTransactionType.AdjustOut:
                    await AdjustLevelAsync(tenantId, transaction.WarehouseId!.Value, item.ProductId, -item.Quantity * multiplier, cancellationToken);
                    break;
                case StockTransactionType.Transfer:
                    await AdjustLevelAsync(tenantId, transaction.FromWarehouseId!.Value, item.ProductId, -item.Quantity * multiplier, cancellationToken);
                    await AdjustLevelAsync(tenantId, transaction.ToWarehouseId!.Value, item.ProductId, item.Quantity * multiplier, cancellationToken);
                    break;
            }
        }
    }

    private async Task AdjustLevelAsync(
        string tenantId,
        Guid warehouseId,
        Guid productId,
        decimal delta,
        CancellationToken cancellationToken)
    {
        var stockLevel = await stockLevelRepository.GetByWarehouseProductAsync(warehouseId, productId, cancellationToken);
        if (stockLevel is null)
        {
            stockLevel = new StockLevel(tenantId, warehouseId, productId);
            await stockLevelRepository.AddAsync(stockLevel, cancellationToken);
        }

        stockLevel.Adjust(delta);
    }

    private static StockTransactionDto ToDto(StockTransaction transaction)
    {
        return new StockTransactionDto
        {
            Id = transaction.Id,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            WarehouseId = transaction.WarehouseId,
            FromWarehouseId = transaction.FromWarehouseId,
            ToWarehouseId = transaction.ToWarehouseId,
            Note = transaction.Note,
            Status = transaction.Status.ToString().ToUpperInvariant(),
            Items = transaction.Items.Select(item => new StockTransactionItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost
            }).ToList()
        };
    }
}

public sealed class StockLevelApplicationService(
    IStockLevelRepository stockLevelRepository,
    ISieveProcessor sieveProcessor) : IStockLevelApplicationService
{
    public Task<PagedResult<StockLevelDto>> ListAsync(StockLevelListQuery query, CancellationToken cancellationToken = default)
    {
        return stockLevelRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<StockLevelDto?> GetByWarehouseProductAsync(Guid warehouseId, Guid productId, CancellationToken cancellationToken = default)
    {
        var stockLevel = await stockLevelRepository.GetByWarehouseProductAsync(warehouseId, productId, cancellationToken);
        return stockLevel is null ? null : ToDto(stockLevel);
    }

    private static StockLevelDto ToDto(StockLevel stockLevel)
    {
        return new StockLevelDto
        {
            WarehouseId = stockLevel.WarehouseId,
            ProductId = stockLevel.ProductId,
            OnHandQuantity = stockLevel.OnHandQuantity
        };
    }
}

public sealed class StockCountSessionApplicationService(
    IStockCountSessionRepository stockCountSessionRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : IStockCountSessionApplicationService
{
    public Task<PagedResult<StockCountSessionDto>> ListAsync(StockCountSessionListQuery query, CancellationToken cancellationToken = default)
    {
        return stockCountSessionRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<StockCountSessionDto?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await stockCountSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        return session is null ? null : ToDto(session);
    }

    public async Task<StockCountSessionDto> CreateAsync(CreateStockCountSessionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var session = new StockCountSession(tenantId, request.WarehouseId, request.SessionName, request.Note);
        await stockCountSessionRepository.AddAsync(session, cancellationToken);
        await stockCountSessionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(session);
    }

    public async Task<StockCountSessionDto?> CompleteAsync(Guid sessionId, CompleteStockCountSessionRequest request, CancellationToken cancellationToken = default)
    {
        var session = await stockCountSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        session.Complete(request.Note);
        await stockCountSessionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(session);
    }

    private static StockCountSessionDto ToDto(StockCountSession session)
    {
        return new StockCountSessionDto
        {
            Id = session.Id,
            WarehouseId = session.WarehouseId,
            SessionName = session.SessionName,
            Note = session.Note,
            Status = session.Status.ToString().ToUpperInvariant(),
            CompletedAtUtc = session.CompletedAtUtc
        };
    }
}
