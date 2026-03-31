using Shouldly;
using PaginationKit.Extensions;

namespace PaginationKit.Tests;

public class PaginationExtensionsTests
{
    private readonly IQueryable<int> _source = Enumerable.Range(1, 100).AsQueryable();

    [Fact]
    public void ShowOnly_AppliesSkipAndTake()
    {
        var result = _source.ShowOnly(skip: 10, take: 5).ToList();

        result.Count.ShouldBe(5);
        result.First().ShouldBe(11);
        result.Last().ShouldBe(15);
    }

    [Fact]
    public void ShowOnly_NullSkipOrTake_ReturnsOriginal()
    {
        var result = _source.ShowOnly(skip: null, take: 5).ToList();
        result.Count.ShouldBe(100);
    }

    [Fact]
    public void ShowOnly_ZeroTake_ReturnsOriginal()
    {
        var result = _source.ShowOnly(skip: 0, take: 0).ToList();
        result.Count.ShouldBe(100);
    }

    [Fact]
    public void ShowOnly_WithPaginationOptions()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 10, pageNumber: 3);
        var (skip, take) = opts.SkipAndTake!.Value;

        var result = _source.ShowOnly(skip, take).ToList();

        result.Count.ShouldBe(10);
        result.First().ShouldBe(21);
    }
}
