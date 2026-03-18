using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace PaginationKit.FastEndpoints;

/// <summary>
/// FastEndpoints pre-processor that extracts pagination from query string
/// and stores PaginationOptions in HttpContext.Items.
/// </summary>
public class PaginationPreProcessor<TRequest> : IPreProcessor<TRequest>
{
    private readonly PaginationRequirement _paginationRequirement;
    private readonly int _pageSize;
    private int? _page = PaginationDefaults.DefaultPage;

    public PaginationPreProcessor(PaginationRequirement paginationRequirement, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        _paginationRequirement = paginationRequirement;
        _pageSize = pageSize;
    }

    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        if (_paginationRequirement != PaginationRequirement.NoPagination)
        {
            if (ctx.HttpContext.Request.Query.TryGetValue("page", out var page))
                _page = Convert.ToInt32(page);

            if (_pageSize > 0)
                ctx.HttpContext.Items.Add(
                    PaginationDefaults.HttpContextItem,
                    PaginationOptions.Create(_paginationRequirement, _pageSize, _page ?? PaginationDefaults.DefaultPage));
        }

        return Task.CompletedTask;
    }
}
