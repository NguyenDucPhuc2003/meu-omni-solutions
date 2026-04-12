using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Cashbook.Domain.Cashbooks;
using Sieve.Services;
using CashbookAggregate = MeuOmni.Modules.Cashbook.Domain.Cashbooks.Cashbook;

namespace MeuOmni.Modules.Cashbook.Application.Cashbooks;

public sealed class CashbookApplicationService(
    ICashbookRepository cashbookRepository,
    ICashTransactionRepository cashTransactionRepository,
    ICashReconciliationRepository cashReconciliationRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ICashbookApplicationService
{
    public Task<PagedResult<CashbookDto>> ListAsync(CashbookListQuery query, CancellationToken cancellationToken = default)
    {
        return cashbookRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CashbookDto?> GetByIdAsync(Guid cashbookId, CancellationToken cancellationToken = default)
    {
        var cashbook = await cashbookRepository.GetByIdAsync(cashbookId, cancellationToken);
        return cashbook is null ? null : ToDto(cashbook);
    }

    public async Task<CashbookDto> CreateAsync(CreateCashbookRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var cashbook = new CashbookAggregate(
            tenantId,
            request.Code,
            request.Name,
            request.CurrencyCode,
            request.OpeningBalance);

        await cashbookRepository.AddAsync(cashbook, cancellationToken);
        await cashbookRepository.SaveChangesAsync(cancellationToken);
        return ToDto(cashbook);
    }

    public async Task<CashbookDto?> UpdateAsync(Guid cashbookId, UpdateCashbookRequest request, CancellationToken cancellationToken = default)
    {
        var cashbook = await cashbookRepository.GetByIdAsync(cashbookId, cancellationToken);
        if (cashbook is null)
        {
            return null;
        }

        cashbook.Update(request.Name, request.CurrencyCode, request.IsActive);
        await cashbookRepository.SaveChangesAsync(cancellationToken);
        return ToDto(cashbook);
    }

    public async Task<CashbookBalanceDto?> GetBalanceAsync(Guid cashbookId, CancellationToken cancellationToken = default)
    {
        var cashbook = await cashbookRepository.GetByIdAsync(cashbookId, cancellationToken);
        if (cashbook is null)
        {
            return null;
        }

        return new CashbookBalanceDto
        {
            CashbookId = cashbook.Id,
            OpeningBalance = cashbook.OpeningBalance,
            CurrentBalance = cashbook.CurrentBalance,
            CurrencyCode = cashbook.CurrencyCode
        };
    }

    public Task<PagedResult<CashTransactionDto>> GetTransactionsAsync(
        Guid cashbookId,
        CashTransactionListQuery query,
        CancellationToken cancellationToken = default)
    {
        return cashTransactionRepository.Query()
            .Where(x => x.CashbookId == cashbookId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, CashTransactionApplicationService.ToDto, cancellationToken);
    }

    public async Task<CashReconciliationDto?> ReconcileAsync(
        Guid cashbookId,
        ReconcileCashbookRequest request,
        CancellationToken cancellationToken = default)
    {
        var cashbook = await cashbookRepository.GetByIdAsync(cashbookId, cancellationToken);
        if (cashbook is null)
        {
            return null;
        }

        var reconciliation = cashbook.Reconcile(request.CountedAmount, request.DifferenceReason, request.ConfirmedBy);
        await cashReconciliationRepository.AddAsync(reconciliation, cancellationToken);
        await cashbookRepository.SaveChangesAsync(cancellationToken);

        return new CashReconciliationDto
        {
            Id = reconciliation.Id,
            CashbookId = reconciliation.CashbookId,
            SystemAmount = reconciliation.SystemAmount,
            CountedAmount = reconciliation.CountedAmount,
            Difference = reconciliation.Difference,
            DifferenceReason = reconciliation.DifferenceReason,
            ConfirmedBy = reconciliation.ConfirmedBy
        };
    }

    private static CashbookDto ToDto(CashbookAggregate cashbook)
    {
        return new CashbookDto
        {
            Id = cashbook.Id,
            Code = cashbook.Code,
            Name = cashbook.Name,
            CurrencyCode = cashbook.CurrencyCode,
            OpeningBalance = cashbook.OpeningBalance,
            CurrentBalance = cashbook.CurrentBalance,
            IsActive = cashbook.IsActive
        };
    }
}

public sealed class CashTransactionApplicationService(
    ICashbookRepository cashbookRepository,
    ICashTransactionRepository cashTransactionRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ICashTransactionApplicationService
{
    public Task<PagedResult<CashTransactionDto>> ListAsync(CashTransactionListQuery query, CancellationToken cancellationToken = default)
    {
        return cashTransactionRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CashTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await cashTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        return transaction is null ? null : ToDto(transaction);
    }

    public async Task<CashTransactionDto> CreateAsync(CreateCashTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var cashbook = await cashbookRepository.GetByIdAsync(request.CashbookId, cancellationToken)
            ?? throw new InvalidOperationException("Cashbook was not found.");

        if (!Enum.TryParse<CashTransactionType>(request.Type, true, out var transactionType))
        {
            throw new InvalidOperationException("Cash transaction type is invalid.");
        }

        var transaction = new CashTransaction(
            tenantId,
            request.CashbookId,
            transactionType,
            request.SubType,
            request.PaymentMethod,
            request.Amount,
            request.CounterpartyType,
            request.CounterpartyId,
            request.SourceDocumentType,
            request.SourceDocumentId,
            request.Note);

        cashbook.ApplyTransaction(transactionType, request.Amount);

        await cashTransactionRepository.AddAsync(transaction, cancellationToken);
        await cashTransactionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    public async Task<CashTransactionDto?> UpdateAsync(Guid transactionId, UpdateCashTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await cashTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        transaction.Update(request.PaymentMethod, request.Note);
        await cashTransactionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    public async Task<CashTransactionDto?> CancelAsync(Guid transactionId, CancelCashTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transaction = await cashTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        if (transaction is null)
        {
            return null;
        }

        var cashbook = await cashbookRepository.GetByIdAsync(transaction.CashbookId, cancellationToken)
            ?? throw new InvalidOperationException("Cashbook was not found.");

        transaction.Cancel(request.Reason);
        cashbook.ReverseTransaction(transaction.Type, transaction.Amount);
        await cashTransactionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    internal static CashTransactionDto ToDto(CashTransaction transaction)
    {
        return new CashTransactionDto
        {
            Id = transaction.Id,
            CashbookId = transaction.CashbookId,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            SubType = transaction.SubType,
            PaymentMethod = transaction.PaymentMethod,
            Amount = transaction.Amount,
            CounterpartyType = transaction.CounterpartyType,
            CounterpartyId = transaction.CounterpartyId,
            SourceDocumentType = transaction.SourceDocumentType,
            SourceDocumentId = transaction.SourceDocumentId,
            Note = transaction.Note,
            Status = transaction.Status.ToString().ToUpperInvariant()
        };
    }
}
