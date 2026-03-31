using Shouldly;
using PaginationKit.Extensions;

namespace PaginationKit.Tests;

public class CursorPaginationExtensionsTests
{
    private record TestItem(int Id, string Name, DateTime CreatedAt);

    private readonly IQueryable<TestItem> _source = new List<TestItem>
    {
        new(1, "Alpha", new DateTime(2024, 1, 1)),
        new(2, "Bravo", new DateTime(2024, 2, 1)),
        new(3, "Charlie", new DateTime(2024, 3, 1)),
        new(4, "Delta", new DateTime(2024, 4, 1)),
        new(5, "Echo", new DateTime(2024, 5, 1)),
    }.AsQueryable();

    [Fact]
    public void AfterCursor_Forward_FiltersItemsAfterCursorValue()
    {
        var result = _source
            .OrderBy(x => x.Id)
            .AfterCursor(x => x.Id, 2)
            .ToList();

        result.Count.ShouldBe(3);
        result[0].Id.ShouldBe(3);
        result[1].Id.ShouldBe(4);
        result[2].Id.ShouldBe(5);
    }

    [Fact]
    public void AfterCursor_Backward_FiltersItemsBeforeCursorValue()
    {
        var result = _source
            .OrderBy(x => x.Id)
            .AfterCursor(x => x.Id, 4, CursorDirection.Backward)
            .ToList();

        result.Count.ShouldBe(3);
        result[0].Id.ShouldBe(1);
        result[1].Id.ShouldBe(2);
        result[2].Id.ShouldBe(3);
    }

    [Fact]
    public void AfterCursor_Forward_WithDateTime()
    {
        var cursorDate = new DateTime(2024, 2, 1);

        var result = _source
            .OrderBy(x => x.CreatedAt)
            .AfterCursor(x => x.CreatedAt, cursorDate)
            .ToList();

        result.Count.ShouldBe(3);
        result[0].Name.ShouldBe("Charlie");
        result[1].Name.ShouldBe("Delta");
        result[2].Name.ShouldBe("Echo");
    }

    [Fact]
    public void AfterCursor_Forward_WithString()
    {
        var result = _source
            .OrderBy(x => x.Name)
            .AfterCursor(x => x.Name, "Charlie")
            .ToList();

        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("Delta");
        result[1].Name.ShouldBe("Echo");
    }

    [Fact]
    public void AfterCursor_Forward_CursorAtEnd_ReturnsEmpty()
    {
        var result = _source
            .OrderBy(x => x.Id)
            .AfterCursor(x => x.Id, 5)
            .ToList();

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void AfterCursor_Backward_CursorAtStart_ReturnsEmpty()
    {
        var result = _source
            .OrderBy(x => x.Id)
            .AfterCursor(x => x.Id, 1, CursorDirection.Backward)
            .ToList();

        result.Count.ShouldBe(0);
    }

    [Fact]
    public void AfterCursor_Forward_WithTake_LimitsResults()
    {
        var result = _source
            .OrderBy(x => x.Id)
            .AfterCursor(x => x.Id, 1)
            .Take(2)
            .ToList();

        result.Count.ShouldBe(2);
        result[0].Id.ShouldBe(2);
        result[1].Id.ShouldBe(3);
    }

    [Fact]
    public void AfterCursor_Forward_CursorBeforeAll_ReturnsAll()
    {
        var result = _source
            .OrderBy(x => x.Id)
            .AfterCursor(x => x.Id, 0)
            .ToList();

        result.Count.ShouldBe(5);
    }
}
