using PaginationKit.Contracts;

namespace PaginationKit;

/// <summary>
/// Cursor-based pagination options with smart defaults based on requirement type.
/// </summary>
public record CursorPaginationOptions : ICursorPaginationOptions
{
    private CursorPaginationOptions(
        PaginationRequirement requirement = PaginationRequirement.NoPagination,
        string? cursor = null,
        int limit = 0,
        CursorDirection direction = CursorDirection.Forward)
    {
        PaginationRequirement = requirement;

        Cursor = requirement switch
        {
            PaginationRequirement.NoPagination => null,
            _ => cursor
        };

        Limit = requirement switch
        {
            PaginationRequirement.NoPagination => 0,
            PaginationRequirement.Required => limit <= 0 ? PaginationDefaults.DefaultPageSize : limit,
            PaginationRequirement.Optional => limit <= 0 ? (cursor is not null ? PaginationDefaults.DefaultPageSize : 0) : limit,
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, null)
        };

        Direction = direction;

        IsPaginated = requirement switch
        {
            PaginationRequirement.NoPagination => false,
            PaginationRequirement.Required => true,
            PaginationRequirement.Optional => Limit > 0,
            _ => false
        };
    }

    public PaginationRequirement PaginationRequirement { get; }
    public string? Cursor { get; }
    public int Limit { get; }
    public bool IsPaginated { get; }
    public CursorDirection Direction { get; }

    /// <summary>
    /// Create cursor pagination options with smart defaults.
    /// </summary>
    /// <param name="requirement">Whether pagination is required, optional, or disabled.</param>
    /// <param name="cursor">Opaque cursor token. Null for the first page.</param>
    /// <param name="limit">Maximum items to return per page.</param>
    /// <param name="direction">Traversal direction (forward or backward from cursor).</param>
    public static CursorPaginationOptions Create(
        PaginationRequirement requirement = PaginationRequirement.NoPagination,
        string? cursor = null,
        int limit = 0,
        CursorDirection direction = CursorDirection.Forward)
        => new(requirement, cursor, limit, direction);
}
