using FluentAssertions;
using PaginationKit.Extensions;

namespace PaginationKit.Tests;

public class FilterExtensionsTests
{
    private readonly IQueryable<int> _source = Enumerable.Range(1, 10).AsQueryable();

    [Fact]
    public void Apply_ConditionTrue_FiltersResults()
    {
        var result = _source.Apply(true, x => x > 5)!.ToList();
        result.Should().HaveCount(5);
        result.Should().AllSatisfy(x => x.Should().BeGreaterThan(5));
    }

    [Fact]
    public void Apply_ConditionFalse_ReturnsOriginal()
    {
        var result = _source.Apply(false, x => x > 5)!.ToList();
        result.Should().HaveCount(10);
    }
}
