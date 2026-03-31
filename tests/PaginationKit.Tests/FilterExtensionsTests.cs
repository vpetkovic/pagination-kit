using Shouldly;
using PaginationKit.Extensions;

namespace PaginationKit.Tests;

public class FilterExtensionsTests
{
    private readonly IQueryable<int> _source = Enumerable.Range(1, 10).AsQueryable();

    [Fact]
    public void Apply_ConditionTrue_FiltersResults()
    {
        var result = _source.Apply(true, x => x > 5)!.ToList();
        result.Count.ShouldBe(5);
        result.ShouldAllBe(x => x > 5);
    }

    [Fact]
    public void Apply_ConditionFalse_ReturnsOriginal()
    {
        var result = _source.Apply(false, x => x > 5)!.ToList();
        result.Count.ShouldBe(10);
    }
}
