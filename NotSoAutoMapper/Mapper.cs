using System;
using System.Linq.Expressions;
using NotSoAutoMapper.ExpressionProcessing;

namespace NotSoAutoMapper
{
    /// <summary>
    ///     The default mapper class, that applies processing transformations to mapping expressions.
    /// </summary>
    /// <seealso cref="IMethodExpressionTransformer" />
    /// <typeparam name="TInput">The input type the mapper is using.</typeparam>
    /// <typeparam name="TResult">The result type the mapper will give.</typeparam>
    public class Mapper<TInput, TResult> : AbstractMapper<TInput, TResult>
        where TInput : notnull
        where TResult : notnull
    {
        /// <summary>
        ///     Creates a mapper with the specified <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The mapping expression to use.</param>
        public Mapper(Expression<Func<TInput, TResult>> expression)
        {
            OriginalExpression = expression ?? throw new ArgumentNullException(nameof(expression));
            Expression = expression.ApplyTransformations();
        }
        
        /// <summary>
        /// The original expression of this mapper, without any transformations applied.
        /// </summary>
        public Expression<Func<TInput, TResult>> OriginalExpression { get; }

        /// <inheritdoc />
        public override Expression<Func<TInput, TResult>> Expression { get; }

        /// <summary>
        /// Creates a new mapper with a new expression but with the same options.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <typeparam name="TNewInput">The new input type.</typeparam>
        /// <typeparam name="TNewResult">The new result type.</typeparam>
        /// <returns>the new mapper with a new expression but with the same options</returns>
        public Mapper<TNewInput, TNewResult> WithExpression<TNewInput, TNewResult>(
            Expression<Func<TNewInput, TNewResult>> expression)
            where TNewInput : notnull
            where TNewResult : notnull
            => new(expression);
    }
}