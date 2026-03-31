using System;
using System.Linq;
using System.Linq.Expressions;

namespace PaginationKit.Extensions;

public static class CursorPaginationExtensions
{
    /// <summary>
    /// Filter a queryable to items after (or before) a cursor value.
    /// Builds a WHERE clause using the key selector and cursor value.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <typeparam name="TKey">Cursor key type (must implement IComparable).</typeparam>
    /// <param name="source">Source queryable.</param>
    /// <param name="keySelector">Expression selecting the cursor key property (e.g., x => x.Id).</param>
    /// <param name="cursorValue">The decoded cursor value to filter from.</param>
    /// <param name="direction">Forward = items after cursor, Backward = items before cursor.</param>
    public static IQueryable<T> AfterCursor<T, TKey>(
        this IQueryable<T> source,
        Expression<Func<T, TKey>> keySelector,
        TKey cursorValue,
        CursorDirection direction = CursorDirection.Forward)
        where TKey : IComparable<TKey>
    {
        var parameter = keySelector.Parameters[0];
        var keyAccess = keySelector.Body;
        var constant = Expression.Constant(cursorValue, typeof(TKey));

        var compareToMethod = typeof(TKey).GetMethod("CompareTo", new[] { typeof(TKey) });
        if (compareToMethod == null)
            throw new InvalidOperationException($"Type {typeof(TKey).Name} does not have a CompareTo({typeof(TKey).Name}) method.");

        var compareCall = Expression.Call(keyAccess, compareToMethod, constant);
        var zero = Expression.Constant(0);

        var comparison = direction == CursorDirection.Forward
            ? Expression.GreaterThan(compareCall, zero)
            : Expression.LessThan(compareCall, zero);

        var lambda = Expression.Lambda<Func<T, bool>>(comparison, parameter);

        return source.Where(lambda);
    }
}
