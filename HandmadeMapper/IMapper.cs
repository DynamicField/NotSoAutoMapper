using System;
using System.Linq.Expressions;

namespace HandmadeMapper
{
    /// <summary>
    /// <para>
    /// Defines how to map an object of type <typeparamref name="TInput"/> to an object of type <typeparamref name="TResult"/>.
    /// </para>
    /// <para>
    /// <b>NOTE:</b> Mappers should be <b>immutable</b>.
    /// </para>
    /// </summary>
    /// <typeparam name="TInput">The input type the mapper is using.</typeparam>
    /// <typeparam name="TResult">The result type the mapper will give.</typeparam>
    public interface IMapper<TInput, TResult> : IMapperExpressionProvider
    {
        /// <summary>
        /// The original expression used, before any transformation occured.
        /// This can be set to the same as <see cref="Expression"/> when not using any transformations.
        /// </summary>
        Expression<Func<TInput, TResult>> OriginalExpression { get; }

        /// <summary>
        /// The expression used to map a <typeparamref name="TInput"/> object to a <typeparamref name="TResult"/> object.
        /// </summary>
        /// <seealso cref="MapperEnumerableExtensions.MapWith{TInput,TResult}(System.Linq.IQueryable{TInput},IMapper{TInput,TResult})"/>
        new Expression<Func<TInput, TResult>> Expression { get; }

        /// <summary>
        /// Maps a <typeparamref name="TInput"/> object to a <typeparamref name="TResult"/> object.
        /// </summary>
        /// <param name="source">The object to map.</param>
        /// <returns>The mapped object.</returns>
        TResult Map(TInput source);

        /// <summary>
        /// Create a new mapper with the same configuration, but with a different expression.
        /// </summary>
        /// <param name="expression">The mapping expression to use.</param>
        /// <returns>A mapper with the specified <paramref name="expression"/></returns>
        IMapper<TInput, TResult> WithExpression(Expression<Func<TInput, TResult>> expression);
    }
}