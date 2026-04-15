namespace MeuOmni.BuildingBlocks.Querying;

public sealed class PagedResult<T>
{
    public required IReadOnlyCollection<T> Items { get; init; }

    public required int Page { get; init; }

    public required int PageSize { get; init; }

    public required int TotalCount { get; init; }

    public string? Sorts { get; init; }

    public string? Filters { get; init; }
}
