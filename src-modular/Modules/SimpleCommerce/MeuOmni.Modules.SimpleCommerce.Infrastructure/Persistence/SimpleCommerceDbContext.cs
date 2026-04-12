using Microsoft.EntityFrameworkCore;
using MeuOmni.BuildingBlocks.Persistence;
using MeuOmni.BuildingBlocks.Security;
using MeuOmni.Modules.SimpleCommerce.Domain.Storefronts.Entities;

namespace MeuOmni.Modules.SimpleCommerce.Infrastructure.Persistence;

public sealed class SimpleCommerceDbContext(
    DbContextOptions<SimpleCommerceDbContext> options,
    ITenantContextAccessor tenantContextAccessor) : TenantAwareDbContext(options, tenantContextAccessor)
{
    public DbSet<Storefront> Storefronts => Set<Storefront>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var storefront = modelBuilder.Entity<Storefront>();
        storefront.ToTable("storefronts");
        storefront.HasKey(x => x.Id);
        storefront.Property(x => x.TenantId).HasMaxLength(64).IsRequired();
        storefront.Property(x => x.Name).HasMaxLength(150).IsRequired();
        storefront.Property(x => x.BaseUrl).HasMaxLength(300).IsRequired();
        storefront.Property(x => x.LinkedSalesChannel).HasMaxLength(50).IsRequired();

        ApplyTenantQueryFilters(modelBuilder);
    }
}
