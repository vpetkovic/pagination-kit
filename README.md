# PaginationKit

A composable .NET pagination and query toolkit. Framework-agnostic core with thin adapters for ASP.NET Core and FastEndpoints.

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
- **ASP.NET Core adapter** (`PaginationKit.AspNetCore`) — Pagination filter, response headers, endpoint builder extensions.
- **FastEndpoints adapter** (`PaginationKit.FastEndpoints`) — Generic `IPreProcessor<TRequest>` for FastEndpoints.

## Packages

| Package | Purpose | Dependencies |
|---------|---------|-------------|
| `PaginationKit` | Pagination models, query extensions, contracts | None (pure .NET) |
| `PaginationKit.AspNetCore` | MVC + Minimal API filter, headers, validation | Core + ASP.NET Core + FluentValidation |
| `PaginationKit.FastEndpoints` | PreProcessor adapter | Core + FastEndpoints |

## Quick Start

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

## Response Headers

When pagination is active, the following headers are added:

| Header | Example | Description |
|--------|---------|-------------|
| `X-Pagination-Page` | `2` | Current page number |
| `X-Pagination-PageSize` | `10` | Items per page |
| `X-Pagination-PageCount` | `5` | Total number of pages |
| `X-Pagination-TotalCount` | `47` | Total items across all pages |
| `Pagination-HasNextPage` | `True` | Whether more pages exist |

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

| Mode | Behavior |
|------|----------|
| `Required` | Always paginates. Defaults to page 1, size 10 if not specified. |
| `Optional` | Only paginates when page/size query params are provided. |
| `NoPagination` | Disabled. Returns all results. |

## Requirements

- .NET 10+

## License

MIT
