using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.Modules.Suppliers.Domain.Suppliers;

namespace MeuOmni.Modules.Suppliers.Application.Suppliers;

public sealed class SupplierListQuery : MeuOmniSieveModel;

public sealed class SupplierDebtTransactionListQuery : MeuOmniSieveModel;

public sealed class CreateSupplierRequest
{
    public string? TenantId { get; init; }
    public string? Code { get; init; }
    public required string Name { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? ContactPerson { get; init; }
}

public sealed class UpdateSupplierRequest
{
    public required string Name { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? ContactPerson { get; init; }
}

public sealed class SupplierStatusActionRequest
{
    public string? Reason { get; init; }
}

public sealed class CreateSupplierDebtTransactionRequest
{
    public string? TenantId { get; init; }
    public Guid SupplierId { get; init; }
    public SupplierDebtTransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public string? SourceDocumentType { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public string? Note { get; init; }
}

public sealed class SupplierDto
{
    public Guid Id { get; init; }
    public string? Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? ContactPerson { get; init; }
    public decimal DebtBalance { get; init; }
    public bool IsActive { get; init; }
}

public sealed class SupplierDebtSummaryDto
{
    public Guid SupplierId { get; init; }
    public decimal DebtBalance { get; init; }
    public decimal TotalIncrease { get; init; }
    public decimal TotalPayment { get; init; }
}

public sealed class SupplierDebtTransactionDto
{
    public Guid Id { get; init; }
    public Guid SupplierId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? SourceDocumentType { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public interface ISupplierApplicationService
{
    Task<PagedResult<SupplierDto>> ListAsync(SupplierListQuery query, CancellationToken cancellationToken = default);
    Task<SupplierDto?> GetByIdAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierDto?> UpdateAsync(Guid supplierId, UpdateSupplierRequest request, CancellationToken cancellationToken = default);
    Task<SupplierDto?> ActivateAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<SupplierDto?> DeactivateAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<SupplierDebtSummaryDto?> GetDebtSummaryAsync(Guid supplierId, CancellationToken cancellationToken = default);
    Task<PagedResult<SupplierDebtTransactionDto>> GetDebtTransactionsAsync(Guid supplierId, SupplierDebtTransactionListQuery query, CancellationToken cancellationToken = default);
}

public interface ISupplierDebtTransactionApplicationService
{
    Task<PagedResult<SupplierDebtTransactionDto>> ListAsync(SupplierDebtTransactionListQuery query, CancellationToken cancellationToken = default);
    Task<SupplierDebtTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);
    Task<SupplierDebtTransactionDto> CreateAsync(CreateSupplierDebtTransactionRequest request, CancellationToken cancellationToken = default);
}
