using System;
using System.Linq.Expressions;

namespace HandmadeMapper.ExpressionProcessing
{
    /// <summary>
    /// Provides extension methods to add default methods to <see cref="IExpressionTransformer"/>.
    /// </summary>
    public static class ExpressionTransformerExtensions
    {
        /// <summary>
        /// Transforms the specified <paramref name="expression"/>, using the specified <paramref name="transformer"/>,
        /// with a default <see cref="MappingContext"/> from the given generic parameters.
        /// </summary>
        /// <inheritdoc cref="IExpressionTransformer.Transform{T}"/>
        public static Expression<Func<TInput, TResult>> Transform<TInput, TResult>(this IExpressionTransformer transformer,
            Expression<Func<TInput, TResult>> expression)
        {
            if (transformer is null)
                throw new ArgumentNullException(nameof(transformer));

            return transformer.Transform(expression, MappingContext.FromTypes<TInput, TResult>());
        }
    }
}