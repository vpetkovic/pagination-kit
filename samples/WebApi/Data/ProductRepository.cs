using WebApi.Models;

namespace WebApi.Data;

public class ProductRepository
{
    private static readonly List<Product> Products = Enumerable.Range(1, 50).Select(i => new Product(
        Id: i,
        Name: $"Product {(char)('A' + (i - 1) % 26)}{i}",
        Category: i % 3 == 0 ? "Electronics" : i % 3 == 1 ? "Books" : "Clothing",
        Price: Math.Round(5.99m + (i * 3.50m), 2),
        CreatedDate: new DateTime(2024, 1, 1).AddDays(i)
    )).ToList();

    public (int totalCount, List<Product> items) GetProducts((int Skip, int Take)? skipAndTake, string? category = null)
    {
        IQueryable<Product> query = Products.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var totalCount = query.Count();

        if (skipAndTake is not null)
            query = query.Skip(skipAndTake.Value.Skip).Take(skipAndTake.Value.Take);

        return (totalCount, query.ToList());
    }

    public IQueryable<Product> Query() => Products.AsQueryable();
}
