using Microsoft.EntityFrameworkCore;
using POS_System.ViewModels.Shared;

namespace POS_System.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(request);

        var totalCount = await query.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return PagedResult<T>.Empty(request.Page, request.PageSize);
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
        var currentPage = Math.Min(request.Page, totalPages);
        var skip = (currentPage - 1) * request.PageSize;

        var items = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, currentPage, request.PageSize, totalCount);
    }
}
