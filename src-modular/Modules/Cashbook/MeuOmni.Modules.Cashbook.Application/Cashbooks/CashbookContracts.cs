using MeuOmni.BuildingBlocks.Querying;

namespace MeuOmni.Modules.Cashbook.Application.Cashbooks;

public sealed class CashbookListQuery : MeuOmniSieveModel;
public sealed class CashTransactionListQuery : MeuOmniSieveModel;

public sealed class CreateCashbookRequest
{
    public string? TenantId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string CurrencyCode { get; init; } = "VND";
    public decimal OpeningBalance { get; init; }
}

public sealed class UpdateCashbookRequest
{
    public required string Name { get; init; }
    public string CurrencyCode { get; init; } = "VND";
    public bool IsActive { get; init; } = true;
}

public sealed class CreateCashTransactionRequest
{
    public string? TenantId { get; init; }
    public Guid CashbookId { get; init; }
    public required string Type { get; init; }
    public string? SubType { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal Amount { get; init; }
    public string? CounterpartyType { get; init; }
    public Guid? CounterpartyId { get; init; }
    public string? SourceDocumentType { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public string? Note { get; init; }
}

public sealed class UpdateCashTransactionRequest
{
    public string? PaymentMethod { get; init; }
    public string? Note { get; init; }
}

public sealed class CancelCashTransactionRequest
{
    public string? Reason { get; init; }
}

public sealed class ReconcileCashbookRequest
{
    public decimal CountedAmount { get; init; }
    public string? DifferenceReason { get; init; }
    public string? ConfirmedBy { get; init; }
}

public sealed class CashbookDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public decimal OpeningBalance { get; init; }
    public decimal CurrentBalance { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CashbookBalanceDto
{
    public Guid CashbookId { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal CurrentBalance { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
}

public sealed class CashTransactionDto
{
    public Guid Id { get; init; }
    public Guid CashbookId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? SubType { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal Amount { get; init; }
    public string? CounterpartyType { get; init; }
    public Guid? CounterpartyId { get; init; }
    public string? SourceDocumentType { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public string? Note { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed class CashReconciliationDto
{
    public Guid Id { get; init; }
    public Guid CashbookId { get; init; }
    public decimal SystemAmount { get; init; }
    public decimal CountedAmount { get; init; }
    public decimal Difference { get; init; }
    public string? DifferenceReason { get; init; }
    public string? ConfirmedBy { get; init; }
}

public interface ICashbookApplicationService
{
    Task<PagedResult<CashbookDto>> ListAsync(CashbookListQuery query, CancellationToken cancellationToken = default);
    Task<CashbookDto?> GetByIdAsync(Guid cashbookId, CancellationToken cancellationToken = default);
    Task<CashbookDto> CreateAsync(CreateCashbookRequest request, CancellationToken cancellationToken = default);
    Task<CashbookDto?> UpdateAsync(Guid cashbookId, UpdateCashbookRequest request, CancellationToken cancellationToken = default);
    Task<CashbookBalanceDto?> GetBalanceAsync(Guid cashbookId, CancellationToken cancellationToken = default);
    Task<PagedResult<CashTransactionDto>> GetTransactionsAsync(Guid cashbookId, CashTransactionListQuery query, CancellationToken cancellationToken = default);
    Task<CashReconciliationDto?> ReconcileAsync(Guid cashbookId, ReconcileCashbookRequest request, CancellationToken cancellationToken = default);
}

public interface ICashTransactionApplicationService
{
    Task<PagedResult<CashTransactionDto>> ListAsync(CashTransactionListQuery query, CancellationToken cancellationToken = default);
    Task<CashTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<CashTransactionDto> CreateAsync(CreateCashTransactionRequest request, CancellationToken cancellationToken = default);
    Task<CashTransactionDto?> UpdateAsync(Guid transactionId, UpdateCashTransactionRequest request, CancellationToken cancellationToken = default);
    Task<CashTransactionDto?> CancelAsync(Guid transactionId, CancelCashTransactionRequest request, CancellationToken cancellationToken = default);
}
