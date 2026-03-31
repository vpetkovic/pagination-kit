# PaginationKit

A composable .NET pagination and query toolkit with **offset-based** and **cursor-based** pagination. Framework-agnostic core with thin adapters for ASP.NET Core and FastEndpoints.

| Package | NuGet | Version | Stats |
| --------------- | --------------- | --------------- | --------------- |
| `PaginationKit` | [`Install-Package PaginationKit`](https://www.nuget.org/packages/PaginationKit/) | ![Nuget](https://img.shields.io/nuget/v/PaginationKit) | ![Nuget](https://img.shields.io/nuget/dt/PaginationKit?label=%20Downloads) |
| `PaginationKit.AspNetCore` | [`Install-Package PaginationKit.AspNetCore`](https://www.nuget.org/packages/PaginationKit.AspNetCore/) | ![Nuget](https://img.shields.io/nuget/v/PaginationKit.AspNetCore) | ![Nuget](https://img.shields.io/nuget/dt/PaginationKit.AspNetCore?label=%20Downloads) |
| `PaginationKit.FastEndpoints` | [`Install-Package PaginationKit.FastEndpoints`](https://www.nuget.org/packages/PaginationKit.FastEndpoints/) | ![Nuget](https://img.shields.io/nuget/v/PaginationKit.FastEndpoints) | ![Nuget](https://img.shields.io/nuget/dt/PaginationKit.FastEndpoints?label=%20Downloads) |

## Why

Every project I've worked on needed pagination, and every single one had its own implementation. Copy-pasted Skip/Take logic, hand-rolled page calculations, pagination headers built differently each time. None of it was reusable across projects.

PaginationKit extracts the patterns I kept rebuilding into a single library with smart defaults, so I never have to write pagination plumbing again.

It separates concerns into composable pieces:

- **Core** (`PaginationKit`) — Pure .NET. No ASP.NET dependency. Works anywhere you have `IQueryable<T>`.
- **ASP.NET Core adapter** (`PaginationKit.AspNetCore`) — Pagination filters, response headers, endpoint builder extensions.
- **FastEndpoints adapter** (`PaginationKit.FastEndpoints`) — Generic `IPreProcessor<TRequest>` for FastEndpoints.

Both **offset/page-based** (traditional page numbers) and **cursor/keyset-based** (infinite scroll, feeds, large datasets) pagination are supported with the same smart defaults pattern.

## Packages

| Package | Purpose | Dependencies |
|---------|---------|-------------|
| `PaginationKit` | Pagination models, query extensions, contracts | None (pure .NET) |
| `PaginationKit.AspNetCore` | MVC + Minimal API filter, headers, validation | Core + ASP.NET Core + FluentValidation |
| `PaginationKit.FastEndpoints` | PreProcessor adapter | Core + FastEndpoints |

## Quick Start — Offset Pagination

### 1. Pagination Options

```csharp
using PaginationKit;

// Required pagination — defaults to page 1, size 10
var opts = PaginationOptions.Create(PaginationRequirement.Required);
// opts.PageNumber = 1, opts.PageSize = 10, opts.SkipAndTake = (0, 10)

// With explicit values
var opts2 = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 25, pageNumber: 3);
// opts2.SkipAndTake = (50, 25)

// Optional — only paginates if page/size are provided
var opts3 = PaginationOptions.Create(PaginationRequirement.Optional);
// opts3.IsPaginated = false (no params given)
```

### 2. Query Extensions

```csharp
using PaginationKit.Extensions;

// Pagination — apply Skip/Take
var paged = query.ShowOnly(skip: 20, take: 10);

// Sorting — multi-column, "-" prefix for descending
var sorted = query.OrderBy("name,-price");

// Conditional filter — only applies when condition is true
var filtered = query
    .Apply(!string.IsNullOrEmpty(category), p => p.Category == category)!
    .Apply(minPrice.HasValue, p => p.Price >= minPrice!.Value)!;
```

### 3. ASP.NET Core Minimal API

```csharp
using PaginationKit;
using PaginationKit.AspNetCore;
using PaginationKit.Extensions;

app.MapGet("/products", (HttpContext ctx) =>
{
    var paging = ctx.GetPaginationOptions();
    var query = db.Products.AsQueryable();

    var totalCount = query.Count();
    var items = query.ShowOnly(paging.SkipAndTake?.Skip, paging.SkipAndTake?.Take).ToList();

    PaginationHeaders.Write(ctx, paging, totalCount);
    return Results.Ok(items);
})
.Pagination(PaginationRequirement.Required, pageSize: 10);
```

### 4. ASP.NET Core MVC Controller

```csharp
[HttpGet]
[PaginationFilter(PaginationRequirement.Required, 10)]
public IActionResult GetProducts()
{
    var paging = HttpContext.GetPaginationOptions();
    // ... same pattern
}
```

### 5. FastEndpoints

```csharp
using PaginationKit.FastEndpoints;

public class GetProductsEndpoint : Endpoint<GetProductsRequest, List<Product>>
{
    public override void Configure()
    {
        Get("/products");
        PreProcessors(new PaginationPreProcessor<GetProductsRequest>(PaginationRequirement.Required, 10));
    }
}
```

### 6. Passing Pagination Down to the Infrastructure Layer

Since the core package has no ASP.NET dependency, `IPaginationOptions` flows naturally through your architecture — from the endpoint down to a repository backed by EF Core.

**Endpoint (API layer):**
```csharp
app.MapGet("/products", (HttpContext ctx, ProductRepository repo) =>
{
    var paging = ctx.GetPaginationOptions();
    var (totalCount, items) = repo.GetProducts(paging, category: "Electronics");

    PaginationHeaders.Write(ctx, paging, totalCount);
    return Results.Ok(items);
})
.Pagination(PaginationRequirement.Required, pageSize: 10);
```

**Repository (Infrastructure layer):**
```csharp
using PaginationKit.Contracts;
using PaginationKit.Extensions;

public class ProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) => _db = db;

    public (int totalCount, List<Product> items) GetProducts(
        IPaginationOptions paging,
        string? category = null)
    {
        var query = _db.Products.AsQueryable()
            .Apply(!string.IsNullOrEmpty(category), p => p.Category == category)!;

        var totalCount = query.Count();

        if (paging.SkipAndTake is not null)
            query = query.ShowOnly(paging.SkipAndTake.Value.Skip, paging.SkipAndTake.Value.Take);

        return (totalCount, query.ToList());
    }
}
```

The repository only depends on `PaginationKit` (core) — it has no idea it's running behind an ASP.NET endpoint.

## Quick Start — Cursor Pagination

Cursor-based pagination uses an opaque token instead of page numbers. It's ideal for infinite scroll, real-time feeds, and large datasets where offset performance degrades.

### 1. Cursor Pagination Options

```csharp
using PaginationKit;

// Required cursor pagination — defaults to limit 10
var opts = CursorPaginationOptions.Create(PaginationRequirement.Required);
// opts.Cursor = null (first page), opts.Limit = 10, opts.IsPaginated = true

// With explicit cursor and limit
var opts2 = CursorPaginationOptions.Create(
    PaginationRequirement.Required, cursor: "abc123", limit: 25);
// opts2.Cursor = "abc123", opts2.Limit = 25

// Backward traversal (previous page)
var opts3 = CursorPaginationOptions.Create(
    PaginationRequirement.Required, cursor: "xyz", limit: 10,
    direction: CursorDirection.Backward);

// Optional — only paginates if cursor or limit is provided
var opts4 = CursorPaginationOptions.Create(PaginationRequirement.Optional);
// opts4.IsPaginated = false (no params given)
```

### 2. Cursor Query Extension

```csharp
using PaginationKit.Extensions;

// Filter to items after a cursor value — works with any IComparable<T> key
var results = query
    .OrderBy(x => x.Id)
    .AfterCursor(x => x.Id, lastSeenId)
    .Take(limit);

// With DateTime cursor
var results = query
    .OrderBy(x => x.CreatedAt)
    .AfterCursor(x => x.CreatedAt, lastSeenDate)
    .Take(limit);

// Backward (previous page)
var results = query
    .OrderByDescending(x => x.Id)
    .AfterCursor(x => x.Id, firstSeenId, CursorDirection.Backward)
    .Take(limit);
```

### 3. ASP.NET Core Minimal API (Cursor)

```csharp
using PaginationKit;
using PaginationKit.AspNetCore;
using PaginationKit.Extensions;

app.MapGet("/feed", (HttpContext ctx) =>
{
    var paging = ctx.GetCursorPaginationOptions();
    var query = db.Posts.OrderBy(p => p.Id).AsQueryable();

    // Decode cursor (you know your key type)
    if (paging.Cursor is not null)
    {
        var afterId = int.Parse(paging.Cursor);
        query = query.AfterCursor(p => p.Id, afterId);
    }

    // Fetch limit + 1 to detect if there are more results
    var items = query.Take(paging.Limit + 1).ToList();
    var hasMore = items.Count > paging.Limit;
    if (hasMore) items.RemoveAt(items.Count - 1);

    var nextCursor = hasMore ? items.Last().Id.ToString() : null;
    CursorPaginationHeaders.Write(ctx, paging, nextCursor);

    return Results.Ok(items);
})
.CursorPagination(PaginationRequirement.Required, limit: 25);
// Reads ?cursor=&limit=&direction= from query string
```

### 4. ASP.NET Core MVC Controller (Cursor)

```csharp
[HttpGet]
[CursorPaginationFilter(PaginationRequirement.Required, 25)]
public IActionResult GetFeed()
{
    var paging = HttpContext.GetCursorPaginationOptions();
    // ... same pattern
}
```

### 5. FastEndpoints (Cursor)

```csharp
using PaginationKit.FastEndpoints;

public class GetFeedEndpoint : Endpoint<GetFeedRequest, List<Post>>
{
    public override void Configure()
    {
        Get("/feed");
        PreProcessors(new CursorPaginationPreProcessor<GetFeedRequest>(PaginationRequirement.Required, 25));
    }
}
```

### 6. Passing Cursor Pagination to Infrastructure

```csharp
// Endpoint (API layer)
app.MapGet("/feed", (HttpContext ctx, PostRepository repo) =>
{
    var paging = ctx.GetCursorPaginationOptions();
    var (items, nextCursor) = repo.GetFeed(paging);

    CursorPaginationHeaders.Write(ctx, paging, nextCursor);
    return Results.Ok(items);
})
.CursorPagination(PaginationRequirement.Required, limit: 25);
```

```csharp
// Repository (Infrastructure layer)
using PaginationKit.Contracts;
using PaginationKit.Extensions;

public class PostRepository
{
    private readonly AppDbContext _db;

    public PostRepository(AppDbContext db) => _db = db;

    public (List<Post> items, string? nextCursor) GetFeed(ICursorPaginationOptions paging)
    {
        var query = _db.Posts.OrderBy(p => p.Id).AsQueryable();

        if (paging.Cursor is not null)
        {
            var afterId = int.Parse(paging.Cursor);
            query = query.AfterCursor(p => p.Id, afterId, paging.Direction);
        }

        var items = query.Take(paging.Limit + 1).ToList();
        var hasMore = items.Count > paging.Limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        var nextCursor = hasMore ? items.Last().Id.ToString() : null;
        return (items, nextCursor);
    }
}
```

## Response Headers — Offset Pagination

When offset pagination is active, the following headers are added:

| Header | Example | Description |
|--------|---------|-------------|
| `X-Pagination-Page` | `2` | Current page number |
| `X-Pagination-PageSize` | `10` | Items per page |
| `X-Pagination-PageCount` | `5` | Total number of pages |
| `X-Pagination-TotalCount` | `47` | Total items across all pages |
| `Pagination-HasNextPage` | `True` | Whether more pages exist |

## Response Headers — Cursor Pagination

When cursor pagination is active, the following headers are added:

| Header | Example | Description |
|--------|---------|-------------|
| `X-Pagination-NextCursor` | `abc123` | Opaque token for next page |
| `X-Pagination-PrevCursor` | `xyz789` | Opaque token for previous page (optional) |
| `X-Pagination-HasMore` | `True` | Whether more results exist |
| `X-Pagination-Limit` | `25` | Items per page |

No `TotalCount` header — cursor pagination intentionally avoids `COUNT(*)` queries for performance.

## Contracts

Request-side interfaces for typed query parameters:

```csharp
public record GetProductsRequest : IPaginated, ISortable
{
    public int? Page { get; set; } = 1;
    public string? SortBy { get; set; }
}
```

## Pagination Modes

`PaginationRequirement` applies to both offset and cursor pagination:

| Mode | Offset Behavior | Cursor Behavior |
|------|----------------|-----------------|
| `Required` | Defaults to page 1, size 10 | Defaults to limit 10, first page |
| `Optional` | Only paginates when page/size provided | Only paginates when cursor/limit provided |
| `NoPagination` | Disabled. Returns all results. | Disabled. Returns all results. |

## Offset vs Cursor — When to Use Which

| Scenario | Use |
|----------|-----|
| Admin dashboard with page numbers | Offset (`PaginationOptions`) |
| Small-medium datasets (<100k rows) | Offset |
| Infinite scroll / "load more" | **Cursor** (`CursorPaginationOptions`) |
| Real-time feeds (social, notifications) | **Cursor** |
| Large datasets (>100k rows) | **Cursor** |
| Public API (GitHub/Stripe style) | **Cursor** |
| Data changes frequently between requests | **Cursor** |

## Requirements

- .NET 10+

## License

MIT
