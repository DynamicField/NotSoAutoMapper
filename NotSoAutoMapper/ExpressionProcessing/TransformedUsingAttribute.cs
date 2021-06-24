using System;

namespace NotSoAutoMapper.ExpressionProcessing
{
    /// <summary>
    /// Defines how should this method call be transformed.
    /// This attribute can be inherited to create custom attributes applying transformations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TransformedUsingAttribute : Attribute
    {
        /// <summary>
        /// Creates a <see cref="TransformedUsingAttribute"/> with the given <paramref name="methodExpressionTransformerType"/>
        /// that must implement <see cref="IMethodExpressionTransformer"/>.
        /// </summary>
        /// <param name="methodExpressionTransformerType">
        /// The type of <see cref="IMethodExpressionTransformer"/>
        /// to use when this method is encountered in a mapper expression.
        /// </param>
        public TransformedUsingAttribute(Type methodExpressionTransformerType)
        {
            MethodExpressionTransformerType = methodExpressionTransformerType
                                              ?? throw new ArgumentNullException(
                                                  nameof(methodExpressionTransformerType));
        }

        /// <summary>
        /// The type of <see cref="IMethodExpressionTransformer"/>
        /// to use when this method is encountered in a mapper expression.
        /// </summary>
        /// <remarks>
        /// An instance of this type is created only once per method signature.
        /// </remarks>
        public Type MethodExpressionTransformerType { get; }
    }
}