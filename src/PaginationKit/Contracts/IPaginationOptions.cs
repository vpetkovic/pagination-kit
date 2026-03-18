namespace PaginationKit.Contracts;

/// <summary>
/// Interface contract for constructing pagination requirements.
/// </summary>
public interface IPaginationOptions
{
    (int Skip, int Take)? SkipAndTake { get; }
    bool IsPaginated { get; }
    int PageNumber { get; }
    int PageSize { get; }
    PaginationRequirement PaginationRequirement { get; }
}
