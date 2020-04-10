using System;
using System.Collections.Generic;
using System.Linq;

namespace HandmadeMapper
{
    /// <summary>
    ///     Provides extension methods for applying mappers on collections.
    /// </summary>
    public static class MapperEnumerableExtensions
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
        public static IQueryable<TResult> MapWith<TInput, TResult>(this IQueryable<TInput> source,
            IMapper<TInput, TResult> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

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
        public static IEnumerable<TResult> MapWith<TInput, TResult>(this IEnumerable<TInput> source,
            IMapper<TInput, TResult> mapper)
        {
            if (mapper is null)
                throw new ArgumentNullException(nameof(mapper));

            return source.Select(mapper.Map);
        }
    }
}