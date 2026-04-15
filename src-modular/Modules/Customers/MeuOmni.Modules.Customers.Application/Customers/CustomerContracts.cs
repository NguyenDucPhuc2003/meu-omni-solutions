using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.Modules.Customers.Domain.Customers;
using System.ComponentModel.DataAnnotations;

namespace MeuOmni.Modules.Customers.Application.Customers;

public sealed class CustomerListQuery : MeuOmniSieveModel;

public sealed class CustomerDebtTransactionListQuery : MeuOmniSieveModel;

public sealed class CustomerGroupListQuery : MeuOmniSieveModel;

public sealed class CreateCustomerRequest
{
    public string? TenantId { get; init; }
    public Guid? StoreId { get; init; }

    [MaxLength(64)]
    public string? Code { get; init; }

    public Guid? GroupId { get; init; }

    [Required]
    [MaxLength(256)]
    public required string FullName { get; init; }

    [Required]
    [MaxLength(32)]
    public string CustomerType { get; init; } = "INDIVIDUAL";

    [MaxLength(256)]
    public string? CompanyName { get; init; }

    [MaxLength(64)]
    public string? TaxCode { get; init; }

    [MaxLength(32)]
    public string? Phone { get; init; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(16)]
    public string? Gender { get; init; }

    public DateOnly? Birthday { get; init; }

    [MaxLength(256)]
    public string? AddressLine { get; init; }

    [MaxLength(128)]
    public string? Ward { get; init; }

    [MaxLength(128)]
    public string? District { get; init; }

    [MaxLength(128)]
    public string? City { get; init; }

    [MaxLength(512)]
    public string? Note { get; init; }
}

public sealed class UpdateCustomerRequest
{
    [Required]
    [MaxLength(256)]
    public required string FullName { get; init; }

    public Guid? GroupId { get; init; }

    [Required]
    [MaxLength(32)]
    public string CustomerType { get; init; } = "INDIVIDUAL";

    [MaxLength(256)]
    public string? CompanyName { get; init; }

    [MaxLength(64)]
    public string? TaxCode { get; init; }

    [MaxLength(32)]
    public string? Phone { get; init; }

    [EmailAddress]
    [MaxLength(256)]
    public string? Email { get; init; }

    [MaxLength(16)]
    public string? Gender { get; init; }

    public DateOnly? Birthday { get; init; }

    [MaxLength(256)]
    public string? AddressLine { get; init; }

    [MaxLength(128)]
    public string? Ward { get; init; }

    [MaxLength(128)]
    public string? District { get; init; }

    [MaxLength(128)]
    public string? City { get; init; }

    [MaxLength(512)]
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

    [MaxLength(32)]
    public string? TxnType { get; init; }

    public CustomerDebtTransactionType? Type { get; init; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal Amount { get; init; }

    [MaxLength(64)]
    public string? SourceDocumentType { get; init; }

    public Guid? SourceDocumentId { get; init; }

    [MaxLength(512)]
    public string? Note { get; init; }
}

public sealed class CreateCustomerGroupRequest
{
    public string? TenantId { get; init; }

    [Required]
    [MaxLength(64)]
    public required string Code { get; init; }

    [Required]
    [MaxLength(256)]
    public required string Name { get; init; }

    [MaxLength(512)]
    public string? Description { get; init; }
}

public sealed class UpdateCustomerGroupRequest
{
    [Required]
    [MaxLength(256)]
    public required string Name { get; init; }

    [MaxLength(512)]
    public string? Description { get; init; }
}

public sealed class CustomerDto
{
    public Guid Id { get; init; }
    public Guid? StoreId { get; init; }
    public string? Code { get; init; }
    public Guid? GroupId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string CustomerType { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? TaxCode { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Gender { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? AddressLine { get; init; }
    public string? Ward { get; init; }
    public string? District { get; init; }
    public string? City { get; init; }
    public string? Note { get; init; }
    public decimal DebtBalance { get; init; }
    public decimal TotalSpent { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CustomerStatisticsDto
{
    public Guid CustomerId { get; init; }
    public int TotalInvoices { get; init; }
    public decimal TotalPurchaseAmount { get; init; }
    public decimal TotalReturnAmount { get; init; }
    public decimal NetPurchaseAmount { get; init; }
    public DateTime? LastPurchaseAt { get; init; }
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

public sealed class CustomerGroupDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
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

    Task<CustomerStatisticsDto?> GetStatisticsAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<PagedResult<CustomerDebtTransactionDto>> GetDebtTransactionsAsync(Guid customerId, CustomerDebtTransactionListQuery query, CancellationToken cancellationToken = default);
}

public interface ICustomerDebtTransactionApplicationService
{
    Task<PagedResult<CustomerDebtTransactionDto>> ListAsync(CustomerDebtTransactionListQuery query, CancellationToken cancellationToken = default);

    Task<CustomerDebtTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task<CustomerDebtTransactionDto> CreateAsync(CreateCustomerDebtTransactionRequest request, CancellationToken cancellationToken = default);
}

public interface ICustomerGroupApplicationService
{
    Task<PagedResult<CustomerGroupDto>> ListAsync(CustomerGroupListQuery query, CancellationToken cancellationToken = default);

    Task<CustomerGroupDto?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<CustomerGroupDto> CreateAsync(CreateCustomerGroupRequest request, CancellationToken cancellationToken = default);

    Task<CustomerGroupDto?> UpdateAsync(Guid groupId, UpdateCustomerGroupRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid groupId, CancellationToken cancellationToken = default);
}
