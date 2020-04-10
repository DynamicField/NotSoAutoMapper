using System;
using System.Linq.Expressions;

namespace HandmadeMapper
{
    /// <summary>
    ///     Defines the expression of a mapper, and options, such as laziness.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public sealed class MapperOptions<TInput, TResult> : IMapperOptions<TInput, TResult>
    {
        /// <summary>
        ///     Creates a new <see cref="MapperOptions{TSource,TTarget}" /> instance with the
        ///     specified <paramref name="expression" /> and the specified laziness (<paramref name="isLazy" />).
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="isLazy">The laziness.</param>
        public MapperOptions(Expression<Func<TInput, TResult>> expression, bool isLazy = false)
        {
            Expression = expression;
            IsLazy = isLazy;
        }

        /// <summary>
        ///     The expression to use in the mapper.
        /// </summary>
        public Expression<Func<TInput, TResult>> Expression { get; set; }

        Expression IMapperOptions.Expression
        {
            get => Expression;
            set => Expression = (Expression<Func<TInput, TResult>>) value;
        }

        /// <summary>
        ///     Whether or not the expression should be transformed lazily
        ///     (when the <see cref="IMapper{TInput,TResult}.Expression" /> getter has been called).
        /// </summary>
        public bool IsLazy { get; set; }
    }

    /// <summary>
    ///     A builder for creating <see cref="MapperOptions{TSource,TTarget}" /> instances.
    /// </summary>
    /// <typeparam name="TInput">The input type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    public sealed class MapperOptionsBuilder<TInput, TResult>
    {
        private Expression<Func<TInput, TResult>>? _expression;
        private bool _isLazy;

        /// <summary>
        ///     Sets the expression to use.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>The same builder.</returns>
        public MapperOptionsBuilder<TInput, TResult> WithExpression(Expression<Func<TInput, TResult>> expression)
        {
            _expression = expression;
            return this;
        }

        /// <summary>
        ///     Sets the laziness of the mapper.
        /// </summary>
        /// <param name="isLazy">The laziness.</param>
        /// <returns>The same builder.</returns>
        public MapperOptionsBuilder<TInput, TResult> WithIsLazy(bool isLazy)
        {
            _isLazy = isLazy;
            return this;
        }

        /// <summary>
        ///     Makes the mapper lazy evaluated.
        /// </summary>
        /// <returns>The same builder.</returns>
        public MapperOptionsBuilder<TInput, TResult> AsLazy()
        {
            return WithIsLazy(true);
        }

        /// <summary>
        ///     Makes the mapper eager evaluated.
        /// </summary>
        /// <returns>The same builder.</returns>
        public MapperOptionsBuilder<TInput, TResult> AsEager()
        {
            return WithIsLazy(false);
        }

        /// <summary>
        ///     Builds the <see cref="MapperOptions{TSource,TTarget}" />.
        /// </summary>
        /// <returns>The <see cref="MapperOptions{TSource,TTarget}" />.</returns>
        public MapperOptions<TInput, TResult> Build()
        {
            if (_expression is null)
                throw new InvalidOperationException(
                    "No expression has been specified for creating a MapperOptions instance.");

            return new MapperOptions<TInput, TResult>(_expression, _isLazy);
        }
    }

    /// <summary>
    ///     Contains helpers methods for <see cref="MapperOptionsBuilder" />.
    /// </summary>
    public static class MapperOptionsBuilder
    {
        /// <summary>
        ///     Creates a new <see cref="MapperOptionsBuilder{TSource,TTarget}" />, with the specified
        ///     <paramref name="expression" /> or not.
        /// </summary>
        /// <typeparam name="TInput">The input type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="expression">The expression to use (optional).</param>
        /// <returns>A new builder with the specified <paramref name="expression" /></returns>
        public static MapperOptionsBuilder<TInput, TResult> Create<TInput, TResult>(
            Expression<Func<TInput, TResult>>? expression = null)
        {
            var builder = new MapperOptionsBuilder<TInput, TResult>();
            if (expression != null)
                builder = builder.WithExpression(expression);
            return builder;
        }

        /// <summary>
        ///     Creates a new <see cref="MapperOptionsBuilder{TSource,TTarget}" />,
        ///     with the properties copied from the specified <paramref name="mapperOptions" />.
        /// </summary>
        /// <typeparam name="TInput">The input type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="mapperOptions">The mapper options to copy the properties from.</param>
        /// <returns>A new builder with the properties copied from <paramref name="mapperOptions" />.</returns>
        public static MapperOptionsBuilder<TInput, TResult> Create<TInput, TResult>(
            IMapperOptions<TInput, TResult> mapperOptions)
        {
            if (mapperOptions == null)
                throw new ArgumentNullException(nameof(mapperOptions));

            return new MapperOptionsBuilder<TInput, TResult>()
                .WithExpression(mapperOptions.Expression)
                .WithIsLazy(mapperOptions.IsLazy);
        }
    }
}