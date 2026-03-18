using FluentAssertions;

namespace PaginationKit.Tests;

public class PaginationOptionsTests
{
    [Fact]
    public void Create_NoPagination_ReturnsZeroed()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.NoPagination);

        opts.IsPaginated.Should().BeFalse();
        opts.PageNumber.Should().Be(0);
        opts.PageSize.Should().Be(0);
        opts.SkipAndTake.Should().BeNull();
    }

    [Fact]
    public void Create_Required_DefaultsToPage1Size10()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required);

        opts.IsPaginated.Should().BeTrue();
        opts.PageNumber.Should().Be(1);
        opts.PageSize.Should().Be(10);
        opts.SkipAndTake.Should().Be((0, 10));
    }

    [Fact]
    public void Create_Required_WithExplicitValues()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 25, pageNumber: 3);

        opts.IsPaginated.Should().BeTrue();
        opts.PageNumber.Should().Be(3);
        opts.PageSize.Should().Be(25);
        opts.SkipAndTake.Should().Be((50, 25)); // (3-1)*25 = 50
    }

    [Fact]
    public void Create_Required_ZeroPageDefaultsTo1()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 5, pageNumber: 0);

        opts.PageNumber.Should().Be(1);
        opts.PageSize.Should().Be(5);
    }

    [Fact]
    public void Create_Required_ZeroSizeDefaultsTo10()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 0, pageNumber: 2);

        opts.PageSize.Should().Be(10);
        opts.PageNumber.Should().Be(2);
    }

    [Fact]
    public void Create_Optional_AllZeros_NotPaginated()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Optional);

        opts.IsPaginated.Should().BeFalse();
        opts.PageNumber.Should().Be(0);
        opts.PageSize.Should().Be(0);
    }

    [Fact]
    public void Create_Optional_WithPageSize_BecomesPaginated()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Optional, pageSize: 20);

        opts.IsPaginated.Should().BeTrue();
        opts.PageNumber.Should().Be(1); // defaults when size > 0
        opts.PageSize.Should().Be(20);
    }

    [Fact]
    public void Create_Optional_WithPageNumber_UsesDefaultSize()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Optional, pageNumber: 2);

        opts.IsPaginated.Should().BeTrue();
        opts.PageNumber.Should().Be(2);
        opts.PageSize.Should().Be(10); // defaults when page > 0
    }

    [Fact]
    public void SkipAndTake_Page1_SkipsZero()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 10, pageNumber: 1);
        opts.SkipAndTake.Should().Be((0, 10));
    }

    [Fact]
    public void SkipAndTake_Page2_SkipsOnePageWorth()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 10, pageNumber: 2);
        opts.SkipAndTake.Should().Be((10, 10));
    }

    [Fact]
    public void SkipAndTake_Page5Size20()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 20, pageNumber: 5);
        opts.SkipAndTake.Should().Be((80, 20)); // (5-1)*20 = 80
    }

    [Fact]
    public void PaginationRequirement_IsPreserved()
    {
        var required = PaginationOptions.Create(PaginationRequirement.Required);
        var optional = PaginationOptions.Create(PaginationRequirement.Optional);

        required.PaginationRequirement.Should().Be(PaginationRequirement.Required);
        optional.PaginationRequirement.Should().Be(PaginationRequirement.Optional);
    }
}
