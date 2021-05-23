using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    /// <summary>
    /// Transforms a <see cref="MethodCallExpression"/>.
    /// </summary>
    public interface IMethodExpressionTransformer
    {
        /// <summary>
        /// Transforms the given method <paramref name="expression"/> into another expression.
        /// </summary>
        /// <param name="expression">The method expression to transform.</param>
        /// <returns>The transformed expression.</returns>
        Expression Transform(MethodCallExpression expression);
    }
}
