using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PaginationKit.AspNetCore.Validation;

namespace PaginationKit.AspNetCore;

/// <summary>
/// Dual-mode cursor pagination filter: works as both MVC IAsyncActionFilter and Minimal API IEndpointFilter.
/// Reads cursor/limit from query string and stores CursorPaginationOptions in HttpContext.Items.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class CursorPaginationFilter : Attribute, IAsyncActionFilter, IEndpointFilter
{
    private readonly PaginationRequirement _paginationRequirement;
    private readonly int _limit;

    public CursorPaginationFilter(PaginationRequirement paginationRequirement, int limit = PaginationDefaults.DefaultLimit)
    {
        _paginationRequirement = paginationRequirement;
        _limit = limit;
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

        var cursorRequest = new CursorPaginationRequestValidationModel
        {
            Limit = isPaginationOptional ? 0 : _limit
        };

        if (context.HttpContext.Request.Query.TryGetValue("cursor", out var cursor))
        {
            cursorRequest.Cursor = cursor.ToString();
        }
        if (context.HttpContext.Request.Query.TryGetValue("limit", out var limit))
        {
            cursorRequest.Limit = int.TryParse(limit, out var limitNum) ? limitNum : null;
        }

        var direction = CursorDirection.Forward;
        if (context.HttpContext.Request.Query.TryGetValue("direction", out var dir))
        {
            if (Enum.TryParse<CursorDirection>(dir, ignoreCase: true, out var parsed))
                direction = parsed;
        }

        var validator = new CursorPaginationRequestValidator();
        var validationResult = await validator.ValidateAsync(cursorRequest);

        if (validationResult.IsValid)
        {
            context.HttpContext.Items.Add(
                PaginationDefaults.CursorHttpContextItem,
                CursorPaginationOptions.Create(
                    _paginationRequirement,
                    cursorRequest.Cursor,
                    cursorRequest.Limit.GetValueOrDefault(isPaginationOptional ? 0 : _limit),
                    direction));
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
            string? cursor = null;
            var limit = _limit;
            var direction = CursorDirection.Forward;

            if (context.HttpContext.Request.Query.TryGetValue("cursor", out var cursorValue))
                cursor = cursorValue.ToString();

            if (context.HttpContext.Request.Query.TryGetValue("limit", out var limitValue))
            {
                if (int.TryParse(limitValue, out var parsed))
                    limit = parsed;
            }

            if (context.HttpContext.Request.Query.TryGetValue("direction", out var dir))
            {
                if (Enum.TryParse<CursorDirection>(dir, ignoreCase: true, out var parsed))
                    direction = parsed;
            }

            if (limit > 0)
                context.HttpContext.Items.Add(
                    PaginationDefaults.CursorHttpContextItem,
                    CursorPaginationOptions.Create(_paginationRequirement, cursor, limit, direction));
        }

        return await next(context);
    }
}
