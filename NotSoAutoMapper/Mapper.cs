using System;
using System.Collections.Generic;
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
    public class Mapper<TInput, TResult> : IMapper<TInput, TResult>
    {
        private Expression<Func<TInput, TResult>>? _actualExpression;
        private readonly Lazy<Func<TInput, TResult>>? _compiledExpression;

        /// <summary>
        ///     Creates a mapper with the specified <paramref name="expression" />.
        /// </summary>
        /// <param name="expression">The mapping expression to use.</param>
        /// <param name="expressionTransformers">
        ///     The expression transformers to use.
        /// </param>
        public Mapper(Expression<Func<TInput, TResult>> expression,
            IEnumerable<IMapperExpressionTransformer>? expressionTransformers = null) : this(expressionTransformers)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        /// <summary>
        ///     Creates a mapper with no expression. The expression should be set using <see cref="UseExpression" />.
        /// </summary>
        /// <param name="expressionTransformers">
        ///     The expression transformers to use.
        /// </param>
        protected Mapper(IEnumerable<IMapperExpressionTransformer>? expressionTransformers = null)
        {
            if (expressionTransformers is not null)
            {
                var before = new List<IMapperExpressionTransformer>();
                var after = new List<IMapperExpressionTransformer>();
                foreach (var expressionTransformer in expressionTransformers)
                {
                    if (expressionTransformer.Position == IMapperExpressionTransformer.RunPosition.Beginning)
                    {
                        before.Add(expressionTransformer);
                    }
                    else
                    {
                        after.Add(expressionTransformer);
                    }
                }

                BeforeExpressionTransformers = before;
                AfterExpressionTransformers = after;
            }
            else
            {
                BeforeExpressionTransformers = Array.Empty<IMapperExpressionTransformer>();
                AfterExpressionTransformers = Array.Empty<IMapperExpressionTransformer>();
            }

            _compiledExpression = new Lazy<Func<TInput, TResult>>(() => Expression.Compile());
        }

        // Private clone constructor
        private Mapper(Expression<Func<TInput, TResult>> expression,
                       IReadOnlyList<IMapperExpressionTransformer> beforeExpressionTransformers,
                       IReadOnlyList<IMapperExpressionTransformer> afterExpressionTransformers)
        {
            BeforeExpressionTransformers = beforeExpressionTransformers;
            AfterExpressionTransformers = afterExpressionTransformers;
            Expression = expression;

            _compiledExpression = new Lazy<Func<TInput, TResult>>(() => Expression.Compile());
        }

        /// <summary>
        ///     The expression processors used to transform the expression located before any other transformers.
        /// </summary>
        protected IReadOnlyList<IMapperExpressionTransformer> BeforeExpressionTransformers { get; }


        /// <summary>
        ///     The expression processors used to transform the expression located after any other transformers.
        /// </summary>
        protected IReadOnlyList<IMapperExpressionTransformer> AfterExpressionTransformers { get; }

        /// <inheritdoc />
        public Expression<Func<TInput, TResult>> OriginalExpression { get; private set; } = null!;

        /// <inheritdoc />
        public Expression<Func<TInput, TResult>> Expression
        {
            get
            {
                _actualExpression ??= ApplyExpressionTransformers(OriginalExpression);
                return _actualExpression;
            }
            // Can be lazy initialized (UseExpression)
            private set
            {
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value == OriginalExpression)
                {
                    return;
                }

                OriginalExpression = value;
                ApplyExpressionTransformers(OriginalExpression);
            }
        }

        /// <inheritdoc />
        LambdaExpression IMapper.Expression => Expression;

        /// <inheritdoc />
        [TransformedUsing(typeof(MapExpressionTransformer))]
        public TResult Map(TInput source)
        {
            if (_compiledExpression is null)
            {
                throw new InvalidOperationException("There is no expression set up.");
            }

            return _compiledExpression.Value(source);
        }

        /// <inheritdoc />
        public virtual IMapper<TInput, TResult> WithExpression(Expression<Func<TInput, TResult>> expression) => new Mapper<TInput, TResult>(expression, BeforeExpressionTransformers, AfterExpressionTransformers);

        /// <summary>
        ///     Sets the mapper's <see cref="Expression" /> to the specified <paramref name="expression" />,
        ///     <b>only when no expression has been given</b>.
        /// </summary>
        /// <param name="expression">The expression to use.</param>
        /// <exception cref="InvalidOperationException">When an exception is already present.</exception>
        protected void UseExpression(Expression<Func<TInput, TResult>> expression)
        {
            if (Expression != null)
            {
                throw new InvalidOperationException("An expression is already present.");
            }

            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        private Expression<T> ApplyExpressionTransformers<T>(Expression<T> expr)
        {
            foreach (var expressionTransformer in BeforeExpressionTransformers)
            {
                expr = expressionTransformer.Transform(expr);
            }

            expr = expr.ApplyTransformations();

            foreach (var expressionTransformer in AfterExpressionTransformers)
            {
                expr = expressionTransformer.Transform(expr);
            }

            return expr;
        }
    }

    /// <summary>
    ///     Provides default behavior for <see cref="Mapper{TInput,TResult}" /> and placeholder methods for expression
    ///     transformations.
    /// </summary>
    /// <seealso cref="Mapper{TInput,TResult}" />
    public static class Mapper
    {
        private const string IncludeError =
            "This method should only be used in expressions, or the IncludeExpressionTransformer has not been used.";
    }
}