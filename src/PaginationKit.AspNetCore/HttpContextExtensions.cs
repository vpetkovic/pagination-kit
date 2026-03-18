using Microsoft.AspNetCore.Http;

namespace PaginationKit.AspNetCore;

public static class HttpContextExtensions
{
    /// <summary>
    /// Retrieve the PaginationOptions stored by the PaginationFilter from HttpContext.Items.
    /// </summary>
    public static PaginationOptions GetPaginationOptions(this HttpContext ctx)
        => (PaginationOptions)ctx.Items[PaginationDefaults.HttpContextItem]!;
}
