using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PaginationKit.AspNetCore.Validation;

namespace PaginationKit.AspNetCore;

/// <summary>
/// Dual-mode pagination filter: works as both MVC IAsyncActionFilter and Minimal API IEndpointFilter.
/// Reads page/size from query string, validates, and stores PaginationOptions in HttpContext.Items.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class PaginationFilter : Attribute, IAsyncActionFilter, IEndpointFilter
{
    private readonly PaginationRequirement _paginationRequirement;
    private readonly int _pageSize;

    public PaginationFilter(PaginationRequirement paginationRequirement, int pageSize = PaginationDefaults.DefaultPageSize)
    {
        _paginationRequirement = paginationRequirement;
        _pageSize = pageSize;
    }

    /// <summary>
    /// MVC controller filter path.
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (_paginationRequirement == PaginationRequirement.NoPagination)
        {
            await next();
            return;
        }

        var isPaginationOptional = _paginationRequirement == PaginationRequirement.Optional;

        var paginationRequest = new PaginationRequestValidationModel
        {
            Page = isPaginationOptional ? 0 : PaginationDefaults.DefaultPage,
            Size = isPaginationOptional ? 0 : _pageSize
        };

        if (context.HttpContext.Request.Query.TryGetValue("page", out var page))
        {
            paginationRequest.Page = int.TryParse(page, out var pageNum) ? pageNum : null;
        }
        if (context.HttpContext.Request.Query.TryGetValue("size", out var size))
        {
            paginationRequest.Size = int.TryParse(size, out var sizeNum) ? sizeNum : null;
        }

        var validator = new PaginationRequestValidator();
        var validationResult = await validator.ValidateAsync(paginationRequest);

        if (validationResult.IsValid)
        {
            context.HttpContext.Items.Add(
                PaginationDefaults.HttpContextItem,
                PaginationOptions.Create(
                    _paginationRequirement,
                    paginationRequest.Size.GetValueOrDefault(isPaginationOptional ? 0 : _pageSize),
                    paginationRequest.Page.GetValueOrDefault(isPaginationOptional ? 0 : PaginationDefaults.DefaultPage)));
        }
        else
        {
            context.Result = new BadRequestObjectResult(
                validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
            return;
        }

        await next();
    }

    /// <summary>
    /// Minimal API endpoint filter path.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_paginationRequirement != PaginationRequirement.NoPagination)
        {
            int? pageValue = PaginationDefaults.DefaultPage;

            if (context.HttpContext.Request.Query.TryGetValue("page", out var page))
                pageValue = Convert.ToInt32(page);

            if (_pageSize > 0)
                context.HttpContext.Items.Add(
                    PaginationDefaults.HttpContextItem,
                    PaginationOptions.Create(_paginationRequirement, _pageSize, pageValue ?? PaginationDefaults.DefaultPage));
        }

        return await next(context);
    }
}
