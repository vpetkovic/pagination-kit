using Microsoft.AspNetCore.Http;

namespace PaginationKit.AspNetCore;

public static class HttpContextExtensions
{
    /// <summary>
    /// Retrieve the PaginationOptions stored by the PaginationFilter from HttpContext.Items.
    /// </summary>
    public static PaginationOptions GetPaginationOptions(this HttpContext ctx)
        => (PaginationOptions)ctx.Items[PaginationDefaults.HttpContextItem]!;

    /// <summary>
    /// Retrieve the CursorPaginationOptions stored by the CursorPaginationFilter from HttpContext.Items.
    /// </summary>
    public static CursorPaginationOptions GetCursorPaginationOptions(this HttpContext ctx)
        => (CursorPaginationOptions)ctx.Items[PaginationDefaults.CursorHttpContextItem]!;
}
