using System.Linq.Expressions;

namespace HandmadeMapper.ExpressionProcessing
{
    /// <summary>
    /// Defines how to transform an expression.
    /// </summary>
    public interface IExpressionTransformer
    {
        /// <summary>
        /// Transforms the specified <paramref name="source"/>, with the specified <paramref name="context"/>.
        /// </summary>
        /// <typeparam name="T">The generic argument of <see cref="Expression{TDelegate}"/>.</typeparam>
        /// <param name="source">The expression to transform.</param>
        /// <param name="context">The mapping context.</param>
        /// <returns>The transformed expression.</returns>
        Expression<T> Transform<T>(Expression<T> source, MappingContext context);
    }
}