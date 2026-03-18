using PaginationKit;
using PaginationKit.AspNetCore;
using PaginationKit.Extensions;
using WebApi.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var repo = new ProductRepository();

// ──────────────────────────────────────────────────
// Basic: Pagination with headers
// GET /products?page=2&size=5
// ──────────────────────────────────────────────────
app.MapGet("/products", (HttpContext ctx) =>
{
    var paging = ctx.GetPaginationOptions();
    var (totalCount, items) = repo.GetProducts(paging.SkipAndTake);

    PaginationHeaders.Write(ctx, paging, totalCount);
    return Results.Ok(items);
})
.Pagination(PaginationRequirement.Required, pageSize: 10);

// ──────────────────────────────────────────────────
// Full: Pagination + Sorting + Conditional Filter
// GET /products/advanced?page=1&size=5&sortBy=name,-price&category=Electronics&minPrice=50
// ──────────────────────────────────────────────────
app.MapGet("/products/advanced", (HttpContext ctx, string? sortBy, string? category, decimal? minPrice) =>
{
    var paging = ctx.GetPaginationOptions();

    var query = repo.Query()
        .Apply(!string.IsNullOrEmpty(category), p => p.Category.Equals(category!, StringComparison.OrdinalIgnoreCase))!
        .Apply(minPrice.HasValue, p => p.Price >= minPrice!.Value)!;

    var totalCount = query.Count();

    if (!string.IsNullOrEmpty(sortBy))
        query = query.OrderBy(sortBy, "id", "name", "price", "createddate", "-id", "-name", "-price", "-createddate");

    if (paging.SkipAndTake is not null)
        query = query.ShowOnly(paging.SkipAndTake.Value.Skip, paging.SkipAndTake.Value.Take);

    PaginationHeaders.Write(ctx, paging, totalCount);
    return Results.Ok(query.ToList());
})
.Pagination(PaginationRequirement.Required, pageSize: 10);

// ──────────────────────────────────────────────────
// Optional pagination (returns all if no page/size provided)
// GET /products/all            → all items, no pagination
// GET /products/all?page=1     → paginated
// ──────────────────────────────────────────────────
app.MapGet("/products/all", (HttpContext ctx) =>
{
    var paging = ctx.GetPaginationOptions();
    var (totalCount, items) = repo.GetProducts(paging.IsPaginated ? paging.SkipAndTake : null);

    if (paging.IsPaginated)
        PaginationHeaders.Write(ctx, paging, totalCount);

    return Results.Ok(items);
})
.Pagination(PaginationRequirement.Optional);

app.Run();
