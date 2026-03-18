using PaginationKit.Contracts;

namespace PaginationKit;

/// <summary>
/// Pagination options with smart defaults based on requirement type.
/// </summary>
public record PaginationOptions : IPaginationOptions
{
    private PaginationOptions(PaginationRequirement requirement = PaginationRequirement.NoPagination, int pageSize = 0, int pageNumber = 0)
    {
        PaginationRequirement = requirement;

        PageNumber = requirement switch
        {
            PaginationRequirement.NoPagination => 0,
            PaginationRequirement.Required => pageNumber == 0 ? 1 : pageNumber,
            PaginationRequirement.Optional => pageNumber == 0 ? (pageSize > 0 ? 1 : 0) : pageNumber,
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, null)
        };

        PageSize = requirement switch
        {
            PaginationRequirement.NoPagination => 0,
            PaginationRequirement.Required => pageSize == 0 ? 10 : pageSize,
            PaginationRequirement.Optional => pageSize == 0 ? (PageNumber > 0 ? 10 : 0) : pageSize,
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, null)
        };

        IsPaginated = !(PageNumber == 0 && PageSize == 0);

        SkipAndTake = IsPaginated ? GetSkipAndTake((PageNumber, PageSize)) : null;
    }

    private static readonly Func<(int page, int size), (int skip, int take)> GetSkipAndTake = ps =>
    {
        var (page, size) = ps;
        page -= 1;
        return (page * size, size);
    };

    public PaginationRequirement PaginationRequirement { get; }
    public (int Skip, int Take)? SkipAndTake { get; }
    public bool IsPaginated { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public static PaginationOptions Create(
        PaginationRequirement requirement = PaginationRequirement.NoPagination,
        int pageSize = 0, int pageNumber = 0)
        => new(requirement, pageSize, pageNumber);
}
