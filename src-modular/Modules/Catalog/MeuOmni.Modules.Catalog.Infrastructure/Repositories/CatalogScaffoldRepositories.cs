using Microsoft.EntityFrameworkCore;
using MeuOmni.Modules.Catalog.Domain.Catalog;
using MeuOmni.Modules.Catalog.Infrastructure.Persistence;

namespace MeuOmni.Modules.Catalog.Infrastructure.Repositories;

public sealed class ProductRepository(CatalogDbContext dbContext) : IProductRepository
{
    public IQueryable<Product> Query()
    {
        return dbContext.Set<Product>().AsQueryable();
    }

    public async Task AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Product>().AddAsync(entity, cancellationToken);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Product>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class ProductPriceRepository(CatalogDbContext dbContext) : IProductPriceRepository
{
    public IQueryable<ProductPrice> Query()
    {
        return dbContext.Set<ProductPrice>().AsQueryable();
    }

    public async Task AddAsync(ProductPrice entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<ProductPrice>().AddAsync(entity, cancellationToken);
    }

    public Task<ProductPrice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ProductPrice>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
public sealed class CategoryRepository(CatalogDbContext dbContext) : ICategoryRepository
{
    public IQueryable<Category> Query()
    {
        return dbContext.Set<Category>().AsQueryable();
    }

    public async Task AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Category>().AddAsync(entity, cancellationToken);
    }

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<Category>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
