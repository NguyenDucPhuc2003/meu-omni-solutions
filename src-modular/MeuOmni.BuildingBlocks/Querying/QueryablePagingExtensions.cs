using Microsoft.EntityFrameworkCore;
using Sieve.Services;

namespace MeuOmni.BuildingBlocks.Querying;

public static class QueryablePagingExtensions
{
    public static async Task<PagedResult<TDestination>> ToPagedResultAsync<TSource, TDestination>(
        this IQueryable<TSource> query,
        MeuOmniSieveModel request,
        ISieveProcessor sieveProcessor,
        Func<TSource, TDestination> selector,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await sieveProcessor
            .Apply(request, query, applyPagination: false)
            .CountAsync(cancellationToken);

        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;

        var entities = await sieveProcessor
            .Apply(request, query)
            .ToListAsync(cancellationToken);

        var items = entities.Select(selector).ToList();

        return new PagedResult<TDestination>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
