using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.Modules.Customers.Domain.Customers;

namespace MeuOmni.Modules.Customers.Application.Customers;

public sealed class CustomerListQuery : MeuOmniSieveModel;

public sealed class CustomerDebtTransactionListQuery : MeuOmniSieveModel;

public sealed class CreateCustomerRequest
{
    public string? TenantId { get; init; }
    public string? Code { get; init; }
    public required string FullName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Note { get; init; }
}

public sealed class UpdateCustomerRequest
{
    public required string FullName { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Note { get; init; }
}

public sealed class CustomerStatusActionRequest
{
    public string? Reason { get; init; }
}

public sealed class CreateCustomerDebtTransactionRequest
{
    public string? TenantId { get; init; }
    public Guid CustomerId { get; init; }
    public CustomerDebtTransactionType Type { get; init; }
    public decimal Amount { get; init; }
    public string? SourceDocumentType { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public string? Note { get; init; }
}

public sealed class CustomerDto
{
    public Guid Id { get; init; }
    public string? Code { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? Note { get; init; }
    public decimal DebtBalance { get; init; }
    public decimal TotalSpent { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CustomerDebtSummaryDto
{
    public Guid CustomerId { get; init; }
    public decimal DebtBalance { get; init; }
    public decimal TotalIncrease { get; init; }
    public decimal TotalPayment { get; init; }
}

public sealed class CustomerDebtTransactionDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string? SourceDocumentType { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public sealed class CustomerPurchaseHistoryItemDto
{
    public string ResourceName { get; init; } = "sales-history";
    public string Message { get; init; } = "Sales history integration will be implemented in SalesChannel.";
}

public interface ICustomerApplicationService
{
    Task<PagedResult<CustomerDto>> ListAsync(CustomerListQuery query, CancellationToken cancellationToken = default);

    Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    Task<CustomerDto?> ActivateAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerDto?> DeactivateAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CustomerPurchaseHistoryItemDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CustomerDebtSummaryDto?> GetDebtSummaryAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<PagedResult<CustomerDebtTransactionDto>> GetDebtTransactionsAsync(Guid customerId, CustomerDebtTransactionListQuery query, CancellationToken cancellationToken = default);
}

public interface ICustomerDebtTransactionApplicationService
{
    Task<PagedResult<CustomerDebtTransactionDto>> ListAsync(CustomerDebtTransactionListQuery query, CancellationToken cancellationToken = default);

    Task<CustomerDebtTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<CustomerDebtTransactionDto> CreateAsync(CreateCustomerDebtTransactionRequest request, CancellationToken cancellationToken = default);
}
