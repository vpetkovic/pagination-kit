using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace PaginationKit.AspNetCore;

public static class EndpointBuilderExtensions
{
    /// <summary>
    /// Add offset-based pagination filter to a minimal API endpoint.
    /// </summary>
    public static RouteHandlerBuilder Pagination(
        this RouteHandlerBuilder builder,
        PaginationRequirement paginationRequirement,
        int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.AddEndpointFilter(new PaginationFilter(paginationRequirement, pageSize));

        return builder;
    }

    /// <summary>
    /// Add cursor-based pagination filter to a minimal API endpoint.
    /// </summary>
    public static RouteHandlerBuilder CursorPagination(
        this RouteHandlerBuilder builder,
        PaginationRequirement paginationRequirement,
        int limit = PaginationDefaults.DefaultLimit)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.AddEndpointFilter(new CursorPaginationFilter(paginationRequirement, limit));

        return builder;
    }
}
