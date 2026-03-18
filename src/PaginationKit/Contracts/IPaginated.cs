namespace PaginationKit.Contracts;

/// <summary>
/// Request-side contract for types that support pagination via a page parameter.
/// </summary>
public interface IPaginated
{
    public int? Page { get; set; }
}
