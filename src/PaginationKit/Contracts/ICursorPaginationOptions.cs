namespace PaginationKit.Contracts;

/// <summary>
/// Interface contract for cursor-based pagination options.
/// Pass this through architecture layers without a concrete dependency.
/// </summary>
public interface ICursorPaginationOptions
{
    string? Cursor { get; }
    int Limit { get; }
    bool IsPaginated { get; }
    CursorDirection Direction { get; }
    PaginationRequirement PaginationRequirement { get; }
}
