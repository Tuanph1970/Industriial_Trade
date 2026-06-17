namespace IndustryTrade.BuildingBlocks.Application.Paging;

/// <summary>Standard page request used by every list/search use case (see docs/design Specification pattern).</summary>
public sealed record PageRequest(int Page = 1, int PageSize = 10, string? Keyword = null)
{
    public const int MaxPageSize = 500;
    public int NormalizedPage => Page < 1 ? 1 : Page;
    public int NormalizedPageSize => PageSize switch
    {
        < 1 => 10,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };
    public int Skip => (NormalizedPage - 1) * NormalizedPageSize;
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
