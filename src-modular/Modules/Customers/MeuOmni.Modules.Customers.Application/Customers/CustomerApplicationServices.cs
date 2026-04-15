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
        var sourceQuery = customerRepository
            .Query()
            .Where(x => query.IncludeInactive || x.IsActive)
            .OrderByDescending(x => x.CreatedAtUtc)
            .AsQueryable();

        return sourceQuery.ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        return customer is null ? null : ToDto(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var normalizedCode = request.Code.Trim();
            var isCodeUsed = await customerRepository.ExistsByCodeAsync(tenantId, normalizedCode, cancellationToken);
            if (isCodeUsed)
            {
                throw new InvalidOperationException($"Customer code '{normalizedCode}' already exists.");
            }
        }

        var customer = new Customer(
            tenantId,
            request.StoreId,
            request.FullName,
            request.Code,
            request.GroupId,
            request.CustomerType,
            request.CompanyName,
            request.TaxCode,
            request.Phone,
            request.Email,
            request.Gender,
            request.Birthday,
            request.AddressLine,
            request.Ward,
            request.District,
            request.City,
            request.Note);

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

        customer.Update(
            request.FullName,
            request.GroupId,
            request.CustomerType,
            request.CompanyName,
            request.TaxCode,
            request.Phone,
            request.Email,
            request.Gender,
            request.Birthday,
            request.AddressLine,
            request.Ward,
            request.District,
            request.City,
            request.Note);
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

    public async Task<CustomerStatisticsDto?> GetStatisticsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
        {
            return null;
        }

        return new CustomerStatisticsDto
        {
            CustomerId = customer.Id,
            TotalInvoices = 0,
            TotalPurchaseAmount = customer.TotalSpent,
            TotalReturnAmount = 0,
            NetPurchaseAmount = customer.TotalSpent,
            LastPurchaseAt = null
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
            StoreId = customer.StoreId,
            Code = customer.Code,
            GroupId = customer.GroupId,
            FullName = customer.FullName,
            CustomerType = customer.CustomerType,
            CompanyName = customer.CompanyName,
            TaxCode = customer.TaxCode,
            Phone = customer.Phone,
            Email = customer.Email,
            Gender = customer.Gender,
            Birthday = customer.Birthday,
            AddressLine = customer.AddressLine,
            Ward = customer.Ward,
            District = customer.District,
            City = customer.City,
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

        var transactionType = ResolveTransactionType(request);

        var transaction = new CustomerDebtTransaction(
            tenantId,
            request.CustomerId,
            transactionType,
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

    private static CustomerDebtTransactionType ResolveTransactionType(CreateCustomerDebtTransactionRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.TxnType))
        {
            if (Enum.TryParse<CustomerDebtTransactionType>(request.TxnType, true, out var parsedType))
            {
                return parsedType;
            }

            throw new InvalidOperationException("Debt transaction type is invalid.");
        }

        if (request.Type.HasValue)
        {
            return request.Type.Value;
        }

        throw new InvalidOperationException("Debt transaction type is required.");
    }
}

public sealed class CustomerGroupApplicationService(
    ICustomerGroupRepository customerGroupRepository,
    ICustomerRepository customerRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ICustomerGroupApplicationService
{
    public Task<PagedResult<CustomerGroupDto>> ListAsync(CustomerGroupListQuery query, CancellationToken cancellationToken = default)
    {
        var sourceQuery = customerGroupRepository
            .Query()
            .Where(x => query.IncludeInactive || x.IsActive)
            .OrderByDescending(x => x.CreatedAtUtc)
            .AsQueryable();

        return sourceQuery.ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CustomerGroupDto?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var group = await customerGroupRepository.GetByIdAsync(groupId, cancellationToken);
        return group is null ? null : ToDto(group);
    }

    public async Task<CustomerGroupDto> CreateAsync(CreateCustomerGroupRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var normalizedCode = request.Code.Trim();
        var isCodeUsed = await customerGroupRepository.ExistsByCodeAsync(tenantId, normalizedCode, cancellationToken);
        if (isCodeUsed)
        {
            throw new InvalidOperationException($"Customer group code '{normalizedCode}' already exists.");
        }

        var group = new CustomerGroup(tenantId, normalizedCode, request.Name, request.Description);

        await customerGroupRepository.AddAsync(group, cancellationToken);
        await customerGroupRepository.SaveChangesAsync(cancellationToken);

        return ToDto(group);
    }

    public async Task<CustomerGroupDto?> UpdateAsync(Guid groupId, UpdateCustomerGroupRequest request, CancellationToken cancellationToken = default)
    {
        var group = await customerGroupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group is null)
        {
            return null;
        }

        group.Update(request.Name, request.Description);
        await customerGroupRepository.SaveChangesAsync(cancellationToken);

        return ToDto(group);
    }

    public async Task<bool> DeleteAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var group = await customerGroupRepository.GetByIdAsync(groupId, cancellationToken);
        if (group is null)
        {
            return false;
        }

        var inUse = await customerRepository.ExistsByGroupIdAsync(groupId, cancellationToken);
        if (inUse)
        {
            throw new InvalidOperationException("Customer group cannot be deleted because it is still assigned to one or more customers.");
        }

        customerGroupRepository.Remove(group);
        await customerGroupRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static CustomerGroupDto ToDto(CustomerGroup group)
    {
        return new CustomerGroupDto
        {
            Id = group.Id,
            Code = group.Code,
            Name = group.Name,
            Description = group.Description,
            IsActive = group.IsActive
        };
    }
}
