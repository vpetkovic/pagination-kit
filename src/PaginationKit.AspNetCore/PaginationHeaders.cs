namespace PaginationKit.AspNetCore;

public struct PaginationHeaders
{
    public const string PaginationPage = "X-Pagination-Page";
    public const string PaginationPageSize = "X-Pagination-PageSize";
    public const string PaginationPageCount = "X-Pagination-PageCount";
    public const string PaginationTotalCount = "X-Pagination-TotalCount";
    public const string PaginationHasNextPage = "Pagination-HasNextPage";

    /// <summary>
    /// Write pagination headers to the HTTP response.
    /// </summary>
    public static void Write(Microsoft.AspNetCore.Http.HttpContext context, PaginationOptions paginationOptions, int itemCount)
    {
        if (!paginationOptions.IsPaginated) return;

        var totalPages = (itemCount / paginationOptions.PageSize) +
                         ((itemCount % paginationOptions.PageSize) == 0 ? 0 : 1);
        var hasNextPage = paginationOptions.PageNumber < totalPages;

        context.Response.Headers.TryAdd(PaginationPage, paginationOptions.PageNumber.ToString());
        context.Response.Headers.TryAdd(PaginationPageSize, paginationOptions.PageSize.ToString());
        context.Response.Headers.TryAdd(PaginationPageCount, totalPages.ToString());
        context.Response.Headers.TryAdd(PaginationTotalCount, itemCount.ToString());
        context.Response.Headers.TryAdd(PaginationHasNextPage, hasNextPage.ToString());
    }
}
