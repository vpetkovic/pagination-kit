using Shouldly;

namespace PaginationKit.Tests;

public class PaginationOptionsTests
{
    [Fact]
    public void Create_NoPagination_ReturnsZeroed()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.NoPagination);

        opts.IsPaginated.ShouldBeFalse();
        opts.PageNumber.ShouldBe(0);
        opts.PageSize.ShouldBe(0);
        opts.SkipAndTake.ShouldBeNull();
    }

    [Fact]
    public void Create_Required_DefaultsToPage1Size10()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required);

        opts.IsPaginated.ShouldBeTrue();
        opts.PageNumber.ShouldBe(1);
        opts.PageSize.ShouldBe(10);
        opts.SkipAndTake.ShouldBe((0, 10));
    }

    [Fact]
    public void Create_Required_WithExplicitValues()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 25, pageNumber: 3);

        opts.IsPaginated.ShouldBeTrue();
        opts.PageNumber.ShouldBe(3);
        opts.PageSize.ShouldBe(25);
        opts.SkipAndTake.ShouldBe((50, 25)); // (3-1)*25 = 50
    }

    [Fact]
    public void Create_Required_ZeroPageDefaultsTo1()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 5, pageNumber: 0);

        opts.PageNumber.ShouldBe(1);
        opts.PageSize.ShouldBe(5);
    }

    [Fact]
    public void Create_Required_ZeroSizeDefaultsTo10()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 0, pageNumber: 2);

        opts.PageSize.ShouldBe(10);
        opts.PageNumber.ShouldBe(2);
    }

    [Fact]
    public void Create_Optional_AllZeros_NotPaginated()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Optional);

        opts.IsPaginated.ShouldBeFalse();
        opts.PageNumber.ShouldBe(0);
        opts.PageSize.ShouldBe(0);
    }

    [Fact]
    public void Create_Optional_WithPageSize_BecomesPaginated()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Optional, pageSize: 20);

        opts.IsPaginated.ShouldBeTrue();
        opts.PageNumber.ShouldBe(1); // defaults when size > 0
        opts.PageSize.ShouldBe(20);
    }

    [Fact]
    public void Create_Optional_WithPageNumber_UsesDefaultSize()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Optional, pageNumber: 2);

        opts.IsPaginated.ShouldBeTrue();
        opts.PageNumber.ShouldBe(2);
        opts.PageSize.ShouldBe(10); // defaults when page > 0
    }

    [Fact]
    public void SkipAndTake_Page1_SkipsZero()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 10, pageNumber: 1);
        opts.SkipAndTake.ShouldBe((0, 10));
    }

    [Fact]
    public void SkipAndTake_Page2_SkipsOnePageWorth()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 10, pageNumber: 2);
        opts.SkipAndTake.ShouldBe((10, 10));
    }

    [Fact]
    public void SkipAndTake_Page5Size20()
    {
        var opts = PaginationOptions.Create(PaginationRequirement.Required, pageSize: 20, pageNumber: 5);
        opts.SkipAndTake.ShouldBe((80, 20)); // (5-1)*20 = 80
    }

    [Fact]
    public void PaginationRequirement_IsPreserved()
    {
        var required = PaginationOptions.Create(PaginationRequirement.Required);
        var optional = PaginationOptions.Create(PaginationRequirement.Optional);

        required.PaginationRequirement.ShouldBe(PaginationRequirement.Required);
        optional.PaginationRequirement.ShouldBe(PaginationRequirement.Optional);
    }
}
