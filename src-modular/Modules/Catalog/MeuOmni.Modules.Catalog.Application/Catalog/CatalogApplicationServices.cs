using MeuOmni.BuildingBlocks.Querying;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.Catalog.Domain.Catalog;
using Sieve.Services;

namespace MeuOmni.Modules.Catalog.Application.Catalog;

public sealed class ProductApplicationService(
    IProductRepository productRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : IProductApplicationService
{
    public Task<PagedResult<ProductDto>> ListAsync(ProductListQuery query, CancellationToken cancellationToken = default)
    {
        return productRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        return product is null ? null : ToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var product = new Product(tenantId, request.Name, request.Code, request.Sku, request.Barcode, request.CategoryId, request.SellPrice);
        await productRepository.AddAsync(product, cancellationToken);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Update(request.Name, request.CategoryId, request.SellPrice);
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToDto(product);
    }

    public async Task<ProductDto?> ActivateAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Activate();
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToDto(product);
    }

    public async Task<ProductDto?> DeactivateAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return null;
        }

        product.Deactivate();
        await productRepository.SaveChangesAsync(cancellationToken);
        return ToDto(product);
    }

    private static ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Code = product.Code,
            Sku = product.Sku,
            Barcode = product.Barcode,
            Name = product.Name,
            CategoryId = product.CategoryId,
            SellPrice = product.SellPrice,
            IsActive = product.IsActive
        };
    }
}

public sealed class ProductPriceApplicationService(
    IProductPriceRepository productPriceRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : IProductPriceApplicationService
{
    public Task<PagedResult<ProductPriceDto>> ListAsync(Guid productId, ProductPriceListQuery query, CancellationToken cancellationToken = default)
    {
        return productPriceRepository.Query()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<ProductPriceDto> CreateAsync(Guid productId, SetProductPriceRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var price = new ProductPrice(tenantId, productId, request.PriceType, request.Price);
        await productPriceRepository.AddAsync(price, cancellationToken);
        await productPriceRepository.SaveChangesAsync(cancellationToken);
        return ToDto(price);
    }

    public async Task<ProductPriceDto?> UpdateAsync(Guid priceId, UpdateProductPriceRequest request, CancellationToken cancellationToken = default)
    {
        var price = await productPriceRepository.GetByIdAsync(priceId, cancellationToken);
        if (price is null)
        {
            return null;
        }

        price.Update(request.Price);
        await productPriceRepository.SaveChangesAsync(cancellationToken);
        return ToDto(price);
    }

    private static ProductPriceDto ToDto(ProductPrice price)
    {
        return new ProductPriceDto
        {
            Id = price.Id,
            ProductId = price.ProductId,
            PriceType = price.PriceType,
            Price = price.Price,
            IsActive = price.IsActive
        };
    }
}

public sealed class CategoryApplicationService(
    ICategoryRepository categoryRepository,
    ITenantContextAccessor tenantContextAccessor,
    ISieveProcessor sieveProcessor) : ICategoryApplicationService
{
    public Task<PagedResult<CategoryDto>> ListAsync(CategoryListQuery query, CancellationToken cancellationToken = default)
    {
        return categoryRepository.Query()
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToPagedResultAsync(query, sieveProcessor, ToDto, cancellationToken);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        return category is null ? null : ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var tenantId = TenantContextGuard.ResolveTenantId(tenantContextAccessor, request.TenantId);
        var category = new Category(tenantId, request.Code, request.Name, request.ParentId);
        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(Guid categoryId, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId, cancellationToken);
        if (category is null)
        {
            return null;
        }

        category.Update(request.Name, request.ParentId);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        return ToDto(category);
    }

    private static CategoryDto ToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            ParentId = category.ParentId,
            IsActive = category.IsActive
        };
    }
}
