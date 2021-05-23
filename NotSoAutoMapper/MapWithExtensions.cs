using NotSoAutoMapper.ExpressionProcessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NotSoAutoMapper
{
    /// <summary>
    ///     Provides extension methods for MapWith methods.
    /// </summary>
    public static class MapWithExtensions
    {
        /// <summary>
        ///     Projects each element of the sequence, using the expression of the specified <paramref name="mapper" />.
        /// </summary>
        /// <typeparam name="TInput">The type of the elements in the source, and of the <paramref name="mapper" />'s input type.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the elements in the projected result, and of the <paramref name="mapper" />'s
        ///     result type.
        /// </typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="mapper">The mapper to apply for each element.</param>
        /// <returns>
        ///     An <see cref="IQueryable{T}" /> whose elements are the result of applying the <paramref name="mapper" /> on
        ///     each element of source.
        /// </returns>
        [TransformedUsing(typeof(MapWithCollectionExpressionTransformer))]
        public static IQueryable<TResult> MapWith<TInput, TResult>(this IQueryable<TInput> source,
            IMapper<TInput, TResult> mapper)
        {
            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return source.Select(mapper.Expression);
        }

        /// <summary>
        ///     Projects each element of the sequence, using the specified <paramref name="mapper" />.
        /// </summary>
        /// <typeparam name="TInput">The type of the elements in the source, and of the <paramref name="mapper" />'s input type.</typeparam>
        /// <typeparam name="TResult">
        ///     The type of the elements in the projected result, and of the <paramref name="mapper" />'s
        ///     result type.
        /// </typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="mapper">The mapper to apply for each element.</param>
        /// <returns>
        ///     An <see cref="IQueryable{T}" /> whose elements are the result of applying the <paramref name="mapper" /> on
        ///     each element of source.
        /// </returns>
        [TransformedUsing(typeof(MapWithCollectionExpressionTransformer))]
        public static IEnumerable<TResult> MapWith<TInput, TResult>(this IEnumerable<TInput> source,
            IMapper<TInput, TResult> mapper)
        {
            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return source.Select(mapper.Map);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        [TransformedUsing(typeof(MapWithObjectExpressionTransformer))]
        public static TResult MapWith<TInput, TResult>(this TInput source, IMapper<TInput, TResult> mapper)
        {
            return mapper.Map(source);
        }
    }
}