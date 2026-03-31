using Shouldly;

namespace PaginationKit.Tests;

public class CursorPaginationOptionsTests
{
    [Fact]
    public void Create_NoPagination_ReturnsZeroed()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.NoPagination);

        opts.IsPaginated.ShouldBeFalse();
        opts.Cursor.ShouldBeNull();
        opts.Limit.ShouldBe(0);
        opts.Direction.ShouldBe(CursorDirection.Forward);
    }

    [Fact]
    public void Create_Required_DefaultsToLimit10()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Required);

        opts.IsPaginated.ShouldBeTrue();
        opts.Cursor.ShouldBeNull();
        opts.Limit.ShouldBe(10);
        opts.Direction.ShouldBe(CursorDirection.Forward);
    }

    [Fact]
    public void Create_Required_WithExplicitValues()
    {
        var opts = CursorPaginationOptions.Create(
            PaginationRequirement.Required,
            cursor: "abc123",
            limit: 25,
            direction: CursorDirection.Backward);

        opts.IsPaginated.ShouldBeTrue();
        opts.Cursor.ShouldBe("abc123");
        opts.Limit.ShouldBe(25);
        opts.Direction.ShouldBe(CursorDirection.Backward);
    }

    [Fact]
    public void Create_Required_ZeroLimitDefaultsTo10()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Required, limit: 0);

        opts.Limit.ShouldBe(10);
        opts.IsPaginated.ShouldBeTrue();
    }

    [Fact]
    public void Create_Required_NegativeLimitDefaultsTo10()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Required, limit: -5);

        opts.Limit.ShouldBe(10);
    }

    [Fact]
    public void Create_Required_NullCursor_FirstPage()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Required, cursor: null, limit: 20);

        opts.IsPaginated.ShouldBeTrue();
        opts.Cursor.ShouldBeNull();
        opts.Limit.ShouldBe(20);
    }

    [Fact]
    public void Create_Optional_NoParams_NotPaginated()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Optional);

        opts.IsPaginated.ShouldBeFalse();
        opts.Cursor.ShouldBeNull();
        opts.Limit.ShouldBe(0);
    }

    [Fact]
    public void Create_Optional_WithCursor_BecomesPaginated()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Optional, cursor: "xyz");

        opts.IsPaginated.ShouldBeTrue();
        opts.Cursor.ShouldBe("xyz");
        opts.Limit.ShouldBe(10); // defaults when cursor provided
    }

    [Fact]
    public void Create_Optional_WithLimit_BecomesPaginated()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Optional, limit: 30);

        opts.IsPaginated.ShouldBeTrue();
        opts.Limit.ShouldBe(30);
    }

    [Fact]
    public void Create_Optional_WithCursorAndLimit()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.Optional, cursor: "abc", limit: 15);

        opts.IsPaginated.ShouldBeTrue();
        opts.Cursor.ShouldBe("abc");
        opts.Limit.ShouldBe(15);
    }

    [Fact]
    public void PaginationRequirement_IsPreserved()
    {
        var required = CursorPaginationOptions.Create(PaginationRequirement.Required);
        var optional = CursorPaginationOptions.Create(PaginationRequirement.Optional);

        required.PaginationRequirement.ShouldBe(PaginationRequirement.Required);
        optional.PaginationRequirement.ShouldBe(PaginationRequirement.Optional);
    }

    [Fact]
    public void NoPagination_CursorIsAlwaysNull()
    {
        var opts = CursorPaginationOptions.Create(PaginationRequirement.NoPagination, cursor: "should-be-ignored");

        opts.Cursor.ShouldBeNull();
        opts.IsPaginated.ShouldBeFalse();
    }
}
