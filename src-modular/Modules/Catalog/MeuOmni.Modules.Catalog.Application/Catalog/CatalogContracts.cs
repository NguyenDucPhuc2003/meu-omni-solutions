using MeuOmni.BuildingBlocks.Querying;

namespace MeuOmni.Modules.Catalog.Application.Catalog;

public sealed class ProductListQuery : MeuOmniSieveModel;
public sealed class ProductPriceListQuery : MeuOmniSieveModel;
public sealed class CategoryListQuery : MeuOmniSieveModel;

public sealed class CreateProductRequest
{
    public string? TenantId { get; init; }
    public string? Code { get; init; }
    public string? Sku { get; init; }
    public string? Barcode { get; init; }
    public required string Name { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal SellPrice { get; init; }
}

public sealed class UpdateProductRequest
{
    public required string Name { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal SellPrice { get; init; }
}

public sealed class SetProductPriceRequest
{
    public string? TenantId { get; init; }
    public string PriceType { get; init; } = "DEFAULT";
    public decimal Price { get; init; }
}

public sealed class UpdateProductPriceRequest
{
    public decimal Price { get; init; }
}

public sealed class ProductStatusActionRequest
{
    public string? Reason { get; init; }
}

public sealed class CreateCategoryRequest
{
    public string? TenantId { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public Guid? ParentId { get; init; }
}

public sealed class UpdateCategoryRequest
{
    public required string Name { get; init; }
    public Guid? ParentId { get; init; }
}

public sealed class ProductDto
{
    public Guid Id { get; init; }
    public string? Code { get; init; }
    public string? Sku { get; init; }
    public string? Barcode { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid? CategoryId { get; init; }
    public decimal SellPrice { get; init; }
    public bool IsActive { get; init; }
}

public sealed class ProductPriceDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string PriceType { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public bool IsActive { get; init; }
}

public sealed class CategoryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Guid? ParentId { get; init; }
    public bool IsActive { get; init; }
}

public interface IProductApplicationService
{
    Task<PagedResult<ProductDto>> ListAsync(ProductListQuery query, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto?> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductDto?> ActivateAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ProductDto?> DeactivateAsync(Guid productId, CancellationToken cancellationToken = default);
}

public interface IProductPriceApplicationService
{
    Task<PagedResult<ProductPriceDto>> ListAsync(Guid productId, ProductPriceListQuery query, CancellationToken cancellationToken = default);
    Task<ProductPriceDto> CreateAsync(Guid productId, SetProductPriceRequest request, CancellationToken cancellationToken = default);
    Task<ProductPriceDto?> UpdateAsync(Guid priceId, UpdateProductPriceRequest request, CancellationToken cancellationToken = default);
}

public interface ICategoryApplicationService
{
    Task<PagedResult<CategoryDto>> ListAsync(CategoryListQuery query, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryDto?> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
}
