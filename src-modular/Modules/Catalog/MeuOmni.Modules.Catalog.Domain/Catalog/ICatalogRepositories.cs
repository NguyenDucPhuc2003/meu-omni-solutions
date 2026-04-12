namespace MeuOmni.Modules.Catalog.Domain.Catalog;

public interface IProductRepository
{
    IQueryable<Product> Query();
    Task AddAsync(Product entity, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IProductPriceRepository
{
    IQueryable<ProductPrice> Query();
    Task AddAsync(ProductPrice entity, CancellationToken cancellationToken = default);
    Task<ProductPrice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICategoryRepository
{
    IQueryable<Category> Query();
    Task AddAsync(Category entity, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
