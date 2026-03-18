namespace PaginationKit.Contracts;

/// <summary>
/// Request-side contract for types that support sorting via a sort parameter.
/// </summary>
public interface ISortable
{
    public string? SortBy { get; set; }
}
