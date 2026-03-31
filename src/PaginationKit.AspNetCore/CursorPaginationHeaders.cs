namespace PaginationKit.AspNetCore;

public struct CursorPaginationHeaders
{
    public const string NextCursor = "X-Pagination-NextCursor";
    public const string PrevCursor = "X-Pagination-PrevCursor";
    public const string HasMore = "X-Pagination-HasMore";
    public const string Limit = "X-Pagination-Limit";

    /// <summary>
    /// Write cursor pagination headers to the HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="paginationOptions">The cursor pagination options used for this request.</param>
    /// <param name="nextCursor">Opaque cursor for the next page, or null if no more results.</param>
    /// <param name="prevCursor">Opaque cursor for the previous page, or null if at the beginning.</param>
    public static void Write(
        Microsoft.AspNetCore.Http.HttpContext context,
        CursorPaginationOptions paginationOptions,
        string? nextCursor,
        string? prevCursor = null)
    {
        if (!paginationOptions.IsPaginated) return;

        context.Response.Headers.TryAdd(Limit, paginationOptions.Limit.ToString());
        context.Response.Headers.TryAdd(HasMore, (nextCursor is not null).ToString());

        if (nextCursor is not null)
            context.Response.Headers.TryAdd(NextCursor, nextCursor);

        if (prevCursor is not null)
            context.Response.Headers.TryAdd(PrevCursor, prevCursor);
    }
}
