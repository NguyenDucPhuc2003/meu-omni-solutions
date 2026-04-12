using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Customers.Domain.Customers;
using Sieve.Services;

namespace MeuOmni.Modules.Customers.Application.Customers;

public sealed class CustomerApplicationService(
    ICustomerRepository customerRepository,
    ICustomerDebtTransactionRepository customerDebtTransactionRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ICustomerApplicationService
{
    public Task<PagedResult<CustomerDto>> ListAsync(CustomerListQuery query, CancellationToken cancellationToken = default)
    {
        return customerRepository
            .Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer is null ? null : ToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var customer = new Customer(tenantId, request.FullName, request.Code, request.Phone, request.Email, request.Address, request.Note);

        await customerRepository.AddAsync(customer, cancellationToken);
        await customerRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.Update(request.FullName, request.Phone, request.Email, request.Address, request.Note);
        await customerRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public async Task<CustomerDto?> ActivateAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.Activate();
        await customerRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public async Task<CustomerDto?> DeactivateAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        customer.Deactivate();
        await customerRepository.SaveChangesAsync(cancellationToken);

        return ToDto(customer);
    }

    public Task<IReadOnlyCollection<CustomerPurchaseHistoryItemDto>> GetPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<CustomerPurchaseHistoryItemDto> items =
        [
            new CustomerPurchaseHistoryItemDto()
        ];

        return Task.FromResult(items);
    }

    public async Task<CustomerDebtSummaryDto?> GetDebtSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        var query = customerDebtTransactionRepository.Query().Where(x => x.CustomerId == customerId);

        var totalIncrease = query
            .Where(x => x.Type == CustomerDebtTransactionType.Increase || x.Type == CustomerDebtTransactionType.Adjustment)
            .Select(x => x.Amount)
            .DefaultIfEmpty(0)
            .Sum();

        var totalPayment = query
            .Where(x => x.Type == CustomerDebtTransactionType.Payment || x.Type == CustomerDebtTransactionType.Offset)
            .Select(x => x.Amount)
            .DefaultIfEmpty(0)
            .Sum();

        return new CustomerDebtSummaryDto
        {
            CustomerId = customer.Id,
            DebtBalance = customer.DebtBalance,
            TotalIncrease = totalIncrease,
            TotalPayment = totalPayment
        };
    }

    public Task<PagedResult<CustomerDebtTransactionDto>> GetDebtTransactionsAsync(Guid customerId, CustomerDebtTransactionListQuery query, CancellationToken cancellationToken = default)
    {
        return customerDebtTransactionRepository
            .Query()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    private static CustomerDto ToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Code = customer.Code,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            Note = customer.Note,
            DebtBalance = customer.DebtBalance,
            TotalSpent = customer.TotalSpent,
            IsActive = customer.IsActive
        };
    }

    private static CustomerDebtTransactionDto ToDto(CustomerDebtTransaction transaction)
    {
        return new CustomerDebtTransactionDto
        {
            Id = transaction.Id,
            CustomerId = transaction.CustomerId,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            Amount = transaction.Amount,
            SourceDocumentType = transaction.SourceDocumentType,
            SourceDocumentId = transaction.SourceDocumentId,
            Note = transaction.Note,
            CreatedAtUtc = transaction.CreatedAtUtc
        };
    }
}

public sealed class CustomerDebtTransactionApplicationService(
    ICustomerDebtTransactionRepository customerDebtTransactionRepository,
    ICustomerRepository customerRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ICustomerDebtTransactionApplicationService
{
    public Task<PagedResult<CustomerDebtTransactionDto>> ListAsync(CustomerDebtTransactionListQuery query, CancellationToken cancellationToken = default)
    {
        return customerDebtTransactionRepository
            .Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CustomerDebtTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await customerDebtTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        return transaction is null ? null : ToDto(transaction);
    }

    public async Task<CustomerDebtTransactionDto> CreateAsync(CreateCustomerDebtTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new InvalidOperationException($"Customer {request.CustomerId} was not found.");

        var transaction = new CustomerDebtTransaction(
            tenantId,
            request.CustomerId,
            request.Type,
            request.Amount,
            request.SourceDocumentType,
            request.SourceDocumentId,
            request.Note);

        customer.ApplyDebtTransaction(transaction);

        await customerDebtTransactionRepository.AddAsync(transaction, cancellationToken);
        await customerDebtTransactionRepository.SaveChangesAsync(cancellationToken);

        return ToDto(transaction);
    }

    private static CustomerDebtTransactionDto ToDto(CustomerDebtTransaction transaction)
    {
        return new CustomerDebtTransactionDto
        {
            Id = transaction.Id,
            CustomerId = transaction.CustomerId,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            Amount = transaction.Amount,
            SourceDocumentType = transaction.SourceDocumentType,
            SourceDocumentId = transaction.SourceDocumentId,
            Note = transaction.Note,
            CreatedAtUtc = transaction.CreatedAtUtc
        };
    }
}
