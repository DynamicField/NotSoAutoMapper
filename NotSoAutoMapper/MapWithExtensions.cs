using NotSoAutoMapper.ExpressionProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            where TInput : notnull 
            where TResult : notnull
        {
            if (mapper == null)
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
            where TInput : notnull 
            where TResult : notnull
        {
            if (mapper is null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            return source.Select(x => mapper.Map(x));
        }
#nullable enable
        /// <summary>
        /// Maps an object using the given mapper.
        /// If the source is null, the projected element is null as well.
        /// </summary>
        /// <typeparam name="TInput">The input type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="source">The source object.</param>
        /// <param name="mapper">The mapper to use.</param>
        /// <returns>The mapped object, or the default value if <paramref name="source"/> is null.</returns>
        [TransformedUsing(typeof(MapWithObjectExpressionTransformer))]
        [return: NotNullIfNotNull("source")]
        public static TResult? MapWith<TInput, TResult>(this TInput? source, IMapper<TInput, TResult> mapper)
            where TInput : notnull
            where TResult : notnull
            => mapper.Map(source);
    }
}