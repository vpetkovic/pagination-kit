namespace PaginationKit.Contracts;

/// <summary>
/// Request-side contract for types that support cursor-based pagination.
/// </summary>
public interface ICursorPaginated
{
    public string? Cursor { get; set; }
}
