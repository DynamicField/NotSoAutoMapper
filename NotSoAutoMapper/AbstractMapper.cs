using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NotSoAutoMapper.ExpressionProcessing;

namespace NotSoAutoMapper
{
    /// <summary>
    /// The base class for implementations of <see cref="IMapper{TInput, TResult}"/>.
    /// </summary>
    /// <typeparam name="TInput">The input type the mapper is using.</typeparam>
    /// <typeparam name="TResult">The result type the mapper will give.</typeparam>
    public abstract class AbstractMapper<TInput, TResult> : IMapper<TInput, TResult>
        where TInput : notnull
        where TResult : notnull
    {
        private readonly Lazy<Func<TInput, TResult>> _compiledExpression;

        /// <summary>
        /// Constructs a new AbstractMapper.
        /// </summary>
        protected AbstractMapper()
        {
            _compiledExpression = new Lazy<Func<TInput, TResult>>(() => Expression.Compile());
        }

        /// <inheritdoc />
        public abstract Expression<Func<TInput, TResult>> Expression { get; }

        /// <inheritdoc />
        [TransformedUsing(typeof(MapExpressionTransformer))]
        [return: NotNullIfNotNull("source")]
        public TResult? Map(TInput? source)
        {
            return EqualityComparer<TInput>.Default.Equals(source!, default!)
                ? default
                : _compiledExpression.Value(source!);
        }

        LambdaExpression IMapper.Expression => Expression;
    }
}