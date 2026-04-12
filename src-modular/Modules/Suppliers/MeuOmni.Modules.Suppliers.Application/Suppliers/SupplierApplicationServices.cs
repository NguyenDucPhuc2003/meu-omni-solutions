using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Suppliers.Domain.Suppliers;
using Sieve.Services;

namespace MeuOmni.Modules.Suppliers.Application.Suppliers;

public sealed class SupplierApplicationService(
    ISupplierRepository supplierRepository,
    ISupplierDebtTransactionRepository supplierDebtTransactionRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ISupplierApplicationService
{
    public Task<PagedResult<SupplierDto>> ListAsync(SupplierListQuery query, CancellationToken cancellationToken = default)
    {
        return supplierRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        return supplier is null ? null : ToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var supplier = new Supplier(tenantId, request.Name, request.Code, request.Phone, request.Email, request.Address, request.ContactPerson);

        await supplierRepository.AddAsync(supplier, cancellationToken);
        await supplierRepository.SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(Guid supplierId, UpdateSupplierRequest request, CancellationToken cancellationToken = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

        supplier.Update(request.Name, request.Phone, request.Email, request.Address, request.ContactPerson);
        await supplierRepository.SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task<SupplierDto?> ActivateAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

        supplier.Activate();
        await supplierRepository.SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task<SupplierDto?> DeactivateAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

        supplier.Deactivate();
        await supplierRepository.SaveChangesAsync(cancellationToken);
        return ToDto(supplier);
    }

    public async Task<SupplierDebtSummaryDto?> GetDebtSummaryAsync(Guid supplierId, CancellationToken cancellationToken = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
        {
            return null;
        }

        var query = supplierDebtTransactionRepository.Query().Where(x => x.SupplierId == supplierId);
        var totalIncrease = query
            .Where(x => x.Type == SupplierDebtTransactionType.Increase || x.Type == SupplierDebtTransactionType.Adjustment)
            .Select(x => x.Amount)
            .DefaultIfEmpty(0)
            .Sum();

        var totalPayment = query
            .Where(x => x.Type == SupplierDebtTransactionType.Payment || x.Type == SupplierDebtTransactionType.Offset)
            .Select(x => x.Amount)
            .DefaultIfEmpty(0)
            .Sum();

        return new SupplierDebtSummaryDto
        {
            SupplierId = supplier.Id,
            DebtBalance = supplier.DebtBalance,
            TotalIncrease = totalIncrease,
            TotalPayment = totalPayment
        };
    }

    public Task<PagedResult<SupplierDebtTransactionDto>> GetDebtTransactionsAsync(Guid supplierId, SupplierDebtTransactionListQuery query, CancellationToken cancellationToken = default)
    {
        return supplierDebtTransactionRepository.Query()
            .Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    private static SupplierDto ToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            ContactPerson = supplier.ContactPerson,
            DebtBalance = supplier.DebtBalance,
            IsActive = supplier.IsActive
        };
    }

    private static SupplierDebtTransactionDto ToDto(SupplierDebtTransaction transaction)
    {
        return new SupplierDebtTransactionDto
        {
            Id = transaction.Id,
            SupplierId = transaction.SupplierId,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            Amount = transaction.Amount,
            SourceDocumentType = transaction.SourceDocumentType,
            SourceDocumentId = transaction.SourceDocumentId,
            Note = transaction.Note,
            CreatedAtUtc = transaction.CreatedAtUtc
        };
    }
}

public sealed class SupplierDebtTransactionApplicationService(
    ISupplierDebtTransactionRepository supplierDebtTransactionRepository,
    ISupplierRepository supplierRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ISupplierDebtTransactionApplicationService
{
    public Task<PagedResult<SupplierDebtTransactionDto>> ListAsync(SupplierDebtTransactionListQuery query, CancellationToken cancellationToken = default)
    {
        return supplierDebtTransactionRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<SupplierDebtTransactionDto?> GetByIdAsync(Guid transactionId, CancellationToken cancellationToken = default)
    {
        var transaction = await supplierDebtTransactionRepository.GetByIdAsync(transactionId, cancellationToken);
        return transaction is null ? null : ToDto(transaction);
    }

    public async Task<SupplierDebtTransactionDto> CreateAsync(CreateSupplierDebtTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var supplier = await supplierRepository.GetByIdAsync(request.SupplierId, cancellationToken)
            ?? throw new InvalidOperationException($"Supplier {request.SupplierId} was not found.");

        var transaction = new SupplierDebtTransaction(
            tenantId,
            request.SupplierId,
            request.Type,
            request.Amount,
            request.SourceDocumentType,
            request.SourceDocumentId,
            request.Note);

        supplier.ApplyDebt(transaction);

        await supplierDebtTransactionRepository.AddAsync(transaction, cancellationToken);
        await supplierDebtTransactionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(transaction);
    }

    private static SupplierDebtTransactionDto ToDto(SupplierDebtTransaction transaction)
    {
        return new SupplierDebtTransactionDto
        {
            Id = transaction.Id,
            SupplierId = transaction.SupplierId,
            Type = transaction.Type.ToString().ToUpperInvariant(),
            Amount = transaction.Amount,
            SourceDocumentType = transaction.SourceDocumentType,
            SourceDocumentId = transaction.SourceDocumentId,
            Note = transaction.Note,
            CreatedAtUtc = transaction.CreatedAtUtc
        };
    }
}
