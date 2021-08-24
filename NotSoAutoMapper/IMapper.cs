using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using NotSoAutoMapper.ExpressionProcessing;

namespace NotSoAutoMapper
{
    /// <summary>
    /// Defines how to map an object of type <typeparamref name="TInput" /> to an object of type
    /// <typeparamref name="TResult" />.
    /// </summary>
    /// <typeparam name="TInput">The input type the mapper is using.</typeparam>
    /// <typeparam name="TResult">The result type the mapper will give.</typeparam>
    public interface IMapper<TInput, TResult> : IMapper
        where TInput : notnull
        where TResult : notnull
    {
        /// <summary>
        ///     The expression used to map a <typeparamref name="TInput" /> object to a <typeparamref name="TResult" /> object.
        /// </summary>
        /// <seealso
        ///     cref="MapWithExtensions.MapWith{TInput,TResult}(System.Linq.IQueryable{TInput},IMapper{TInput,TResult})" />
        new Expression<Func<TInput, TResult>> Expression { get; }

        /// <summary>
        ///     Maps a <typeparamref name="TInput" /> object to a <typeparamref name="TResult" /> object.
        ///     If the source is equal to its default value, no mapping is applied and the default value
        ///     will be returned.
        /// </summary>
        /// <param name="source">The object to map.</param>
        /// <returns>The mapped object.</returns>
        [TransformedUsing(typeof(MapExpressionTransformer))]
        [return: NotNullIfNotNull("source")]
        TResult? Map(TInput? source);
    }

    /// <summary>
    ///     Defines basic mapping expressions.
    /// </summary>
    public interface IMapper
    {
        /// <summary>
        ///     The expression used to map an object to another.
        /// </summary>
        LambdaExpression Expression { get; }
    }
}