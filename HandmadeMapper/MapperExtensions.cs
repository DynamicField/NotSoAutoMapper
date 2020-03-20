using System;
using System.Linq.Expressions;

namespace HandmadeMapper
{
    /// <summary>
    /// Provides extension methods for <see cref="IMapper{TInput,TResult}"/>.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Creates a new mapper with the expression of the specified <paramref name="mapper"/> with the specified <paramref name="mergeExtension"/>,
        /// using <see cref="ExpressionExtensions.Merge{T}"/>.
        /// </summary>
        /// <typeparam name="TInput">The input type of the mapper.</typeparam>
        /// <typeparam name="TResult">The result type of the mapper.</typeparam>
        /// <param name="mapper">The mapper containing the expression to merge.</param>
        /// <param name="mergeExtension">The expression that will be merged with the <paramref name="mapper"/>'s expression.</param>
        /// <returns>A mapper with the merged expression, created using <see cref="IMapper{TInput,TResult}.WithExpression"/>.</returns>
        /// <seealso cref="ExpressionExtensions.Merge{T}"/>
        /// <seealso cref="IMapper{TInput,TResult}.WithExpression"/>
        public static IMapper<TInput, TResult> Merge<TInput, TResult>(this IMapper<TInput, TResult> mapper, Expression<Func<TInput, TResult>> mergeExtension)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            var originalExpression = mapper.Expression;
            var mergedExpression = originalExpression.Merge(mergeExtension);
            return mapper.WithExpression(mergedExpression);
        }
    }
}