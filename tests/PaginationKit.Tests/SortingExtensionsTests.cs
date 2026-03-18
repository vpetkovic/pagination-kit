using FluentAssertions;
using PaginationKit.Extensions;

namespace PaginationKit.Tests;

public class SortingExtensionsTests
{
    private record TestItem(int Id, string Name, DateTime Date);

    private readonly IQueryable<TestItem> _source = new List<TestItem>
    {
        new(3, "Charlie", new DateTime(2024, 3, 1)),
        new(1, "Alice", new DateTime(2024, 1, 1)),
        new(2, "Bob", new DateTime(2024, 2, 1)),
    }.AsQueryable();

    [Fact]
    public void OrderBy_SingleAscending()
    {
        var result = _source.OrderBy("name").ToList();

        result[0].Name.Should().Be("Alice");
        result[1].Name.Should().Be("Bob");
        result[2].Name.Should().Be("Charlie");
    }

    [Fact]
    public void OrderBy_SingleDescending()
    {
        var result = _source.OrderBy("-name").ToList();

        result[0].Name.Should().Be("Charlie");
        result[1].Name.Should().Be("Bob");
        result[2].Name.Should().Be("Alice");
    }

    [Fact]
    public void OrderBy_MultipleColumns()
    {
        var items = new List<TestItem>
        {
            new(1, "Alice", new DateTime(2024, 2, 1)),
            new(2, "Alice", new DateTime(2024, 1, 1)),
            new(3, "Bob", new DateTime(2024, 1, 1)),
        }.AsQueryable();

        var result = items.OrderBy("name,-date").ToList();

        result[0].Id.Should().Be(1); // Alice, 2024-02 (desc date)
        result[1].Id.Should().Be(2); // Alice, 2024-01
        result[2].Id.Should().Be(3); // Bob
    }

    [Fact]
    public void OrderBy_CaseInsensitive()
    {
        var result = _source.OrderBy("NAME").ToList();

        result[0].Name.Should().Be("Alice");
    }

    [Fact]
    public void OrderBy_EmptyString_NoChange()
    {
        var result = _source.OrderBy("").ToList();
        result[0].Id.Should().Be(3); // original order preserved
    }
}
