using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PaginationKit.FastEndpoints;

/// <summary>
/// FastEndpoints pre-processor that extracts cursor pagination from query string
/// and stores CursorPaginationOptions in HttpContext.Items.
/// </summary>
public class CursorPaginationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly PaginationRequirement _paginationRequirement;
    private readonly int _limit;

    public CursorPaginationPreProcessor(PaginationRequirement paginationRequirement, int limit = PaginationDefaults.DefaultLimit)
    {
        _paginationRequirement = paginationRequirement;
        _limit = limit;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        if (_paginationRequirement != PaginationRequirement.NoPagination)
        {
            string? cursor = null;
            var limit = _limit;
            var direction = CursorDirection.Forward;

            if (ctx.HttpContext.Request.Query.TryGetValue("cursor", out var cursorValue))
                cursor = cursorValue.ToString();

            if (ctx.HttpContext.Request.Query.TryGetValue("limit", out var limitValue))
            {
                if (int.TryParse(limitValue, out var parsed))
                    limit = parsed;
            }

            if (ctx.HttpContext.Request.Query.TryGetValue("direction", out var dir))
            {
                if (Enum.TryParse<CursorDirection>(dir, ignoreCase: true, out var parsed))
                    direction = parsed;
            }

            if (limit > 0)
                ctx.HttpContext.Items.Add(
                    PaginationDefaults.CursorHttpContextItem,
                    CursorPaginationOptions.Create(_paginationRequirement, cursor, limit, direction));
        }

        return Task.CompletedTask;
    }
}
