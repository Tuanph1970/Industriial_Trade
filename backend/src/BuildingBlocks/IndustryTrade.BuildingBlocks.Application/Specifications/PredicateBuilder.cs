using System.Linq.Expressions;

namespace IndustryTrade.BuildingBlocks.Application.Specifications;

/// <summary>Combines EF-translatable predicate expressions with AND/OR (rebinding parameters).</summary>
public static class PredicateBuilder
{
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right) =>
        Combine(left, right, Expression.OrElse);

    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right) =>
        Combine(left, right, Expression.AndAlso);

    private static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        Func<Expression, Expression, BinaryExpression> merge)
    {
        var parameter = Expression.Parameter(typeof(T));
        var body = merge(
            Rebind(left.Body, left.Parameters[0], parameter),
            Rebind(right.Body, right.Parameters[0], parameter));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression Rebind(Expression body, ParameterExpression from, ParameterExpression to) =>
        new ReplaceVisitor(from, to).Visit(body);

    private sealed class ReplaceVisitor(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == from ? to : base.VisitParameter(node);
    }
}
