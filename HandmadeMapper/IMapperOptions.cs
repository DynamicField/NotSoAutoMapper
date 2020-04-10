using System;
using System.Linq.Expressions;

namespace HandmadeMapper
{
    /// <summary>
    ///     The strongly-typed version of <see cref="IMapperOptions" />.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public interface IMapperOptions<TInput, TResult> : IMapperOptions
    {
        /// <inheritdoc cref="Expression" />
        new Expression<Func<TInput, TResult>> Expression { get; set; }
    }

    /// <summary>
    ///     The base interface for defining mapper options.
    /// </summary>
    public interface IMapperOptions
    {
        /// <summary>
        ///     The expression to use in the mapper.
        /// </summary>
        Expression Expression { get; set; }

        /// <summary>
        ///     Whether or not the expression should be transformed lazily
        ///     (when the <see cref="IMapper{TInput,TResult}.Expression" /> getter has been called).
        /// </summary>
        bool IsLazy { get; set; }
    }
}