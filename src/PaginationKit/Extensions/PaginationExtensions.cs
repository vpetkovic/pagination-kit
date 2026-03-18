using System.Diagnostics;
using System.Linq.Expressions;

namespace PaginationKit.Extensions;

public static class PaginationExtensions
{
    /// <summary>
    /// Apply Skip/Take to an IQueryable. Typically used with <see cref="PaginationOptions.SkipAndTake"/>.
    /// </summary>
    public static IQueryable<T> ShowOnly<T>(this IQueryable<T> source, int? skip = 0, int? take = 0)
    {
        if (skip is null || take is null) return source;

        if (take > 0)
            return source.Skip((int)skip).Take((int)take);

        return source;
    }

    /// <summary>
    /// Order a queryable by a comma-separated list of property names.
    /// Prefix with "-" for descending order (e.g., "name,-date").
    /// </summary>
    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderBy, params string[]? allowedColumns) where T : class
    {
        var queryExpression = source.Expression;
        queryExpression = queryExpression.ApplyOrderBy(orderBy, allowedColumns);

        if (queryExpression.CanReduce) queryExpression = queryExpression.Reduce();

        return source.Provider.CreateQuery<T>(queryExpression);
    }

    /// <summary>
    /// Order an enumerable by a comma-separated list of property names.
    /// Prefix with "-" for descending order (e.g., "name,-date").
    /// </summary>
    public static IEnumerable<T> OrderBy<T>(
        this IEnumerable<T> source,
        string orderBy,
        string[]? allowedColumns = null) where T : class
    {
        var s = source.AsQueryable();
        var queryExpression = s.Expression;
        queryExpression = queryExpression.ApplyOrderBy(orderBy, allowedColumns);

        if (queryExpression.CanReduce) queryExpression = queryExpression.Reduce();

        return s.Provider.CreateQuery<T>(queryExpression);
    }

    /// <summary>
    /// Apply a Where predicate only when the condition is true.
    /// </summary>
    public static IQueryable<T>? Apply<T>(this IQueryable<T> source, bool condition, Expression<Func<T, bool>> predicate)
    {
        if (condition) return source.Where(predicate);
        return source;
    }
}

internal static class ExpressionExtensions
{
    internal static Expression ApplyOrderBy(this Expression source, string orderBy, string[]? allowed = null)
    {
        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            var orderBys = orderBy.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < orderBys.Length; i++)
            {
                if (allowed is null || allowed.Length == 0 || allowed.Contains(orderBys[i]))
                    source = AddOrderBy(source, orderBys[i], i);
            }
        }

        return source;
    }

    private static Expression AddOrderBy(Expression source, string orderBy, int index)
    {
        string parameterPath = orderBy.Replace("-", "");
        string orderByMethodName = index == 0 ? "OrderBy" : "ThenBy";
        orderByMethodName += orderBy.StartsWith("-") ? "Descending" : "";

        var sourceType = source.Type.GetGenericArguments().First();
        var parameterExpression = Expression.Parameter(sourceType, "p");
        var orderByExpression = BuildPropertyPathExpression(parameterExpression, parameterPath);

        if (orderByExpression.propertyFound)
        {
            var orderByFuncType = typeof(Func<,>).MakeGenericType(sourceType, orderByExpression.expression.Type);
            var orderByLambda = Expression.Lambda(orderByFuncType, orderByExpression.expression, new ParameterExpression[] { parameterExpression });

            if (orderByExpression.expression.Type.Name.Contains("IEnumerable")) return source;
            if (orderByExpression.expression.Type.IsArray) return source;
            source = Expression.Call(typeof(Queryable), orderByMethodName, new Type[] { sourceType, orderByExpression.expression.Type }, source, orderByLambda);
        }

        return source;
    }

    private static (Expression expression, bool propertyFound) BuildPropertyPathExpression(Expression rootExpression, string propertyPath)
    {
        var parts = propertyPath.Split(new[] { '.' }, 2);
        var currentProperty = parts[0];
        var propertyDescription = rootExpression.Type.GetProperty(currentProperty, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

        Debug.Assert(propertyDescription != null, $"Cannot find property {rootExpression.Type.Name}.{currentProperty}. The root expression is {rootExpression} and the full path would be {propertyPath}.");
        if (propertyDescription == null) return (null, false)!;

        var propExpr = Expression.Property(rootExpression, propertyDescription);
        if (parts.Length > 1)
            return BuildPropertyPathExpression(propExpr, parts[1]);

        return (propExpr, true);
    }
}
