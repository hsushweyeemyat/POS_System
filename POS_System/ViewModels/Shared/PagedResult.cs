namespace POS_System.ViewModels.Shared;

public class PagedResult<T>
{
    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page < 1 ? PaginationRequest.DefaultPage : page;
        PageSize = pageSize < 1 ? PaginationRequest.DefaultPageSize : pageSize;
        TotalCount = totalCount < 0 ? 0 : totalCount;
    }

    public IReadOnlyList<T> Items { get; }

    public int Page { get; }

    public int PageSize { get; }

    public int TotalCount { get; }

    public int TotalPages => TotalCount == 0
        ? 1
        : (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public int FirstItemNumber => TotalCount == 0 ? 0 : ((Page - 1) * PageSize) + 1;

    public int LastItemNumber => TotalCount == 0 ? 0 : Math.Min(Page * PageSize, TotalCount);

    public static PagedResult<T> Empty(int page = PaginationRequest.DefaultPage, int pageSize = PaginationRequest.DefaultPageSize)
        => new(Array.Empty<T>(), page, pageSize, 0);
}
