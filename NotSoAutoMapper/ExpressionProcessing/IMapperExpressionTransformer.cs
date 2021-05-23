using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    /// <summary>
    ///     Defines how to transform an expression contained in a <see cref="Mapper{TInput, TResult}"/>.
    /// </summary>
    public interface IMapperExpressionTransformer
    {
        /// <summary>
        ///     Transforms the specified <paramref name="source" />.
        /// </summary>
        /// <typeparam name="T">The generic argument of <see cref="Expression{TDelegate}" />.</typeparam>
        /// <param name="source">The expression to transform.</param>
        /// 
        /// <returns>The transformed expression.</returns>
        Expression<T> Transform<T>(Expression<T> source);

        /// <summary>
        /// The position of the transformer in the transformation pipeline.
        /// </summary>
        RunPosition Position { get; }

        /// <summary>
        /// Represents when should the <see cref="IMethodExpressionTransformer"/> be executed.
        /// </summary>
        public enum RunPosition
        {
            /// <summary>
            /// Should be executed at the beginning, before any types from <see cref="TransformedUsingAttribute"/>.
            /// </summary>
            Beginning,
            /// <summary>
            /// Should be executed at the end, after any types from <see cref="TransformedUsingAttribute"/>.
            /// </summary>
            End
        }
    }
}