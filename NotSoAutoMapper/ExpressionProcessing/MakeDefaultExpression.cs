using System;
using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal static class MakeDefaultExpression
    {
        /// <summary>
        /// Makes a testable default expression for the given expression.
        /// </summary>
        /// <param name="expression">The expression to make a default expression for.</param>
        /// <returns>The default expression.</returns>
        public static Expression For(Expression expression) => For(expression.Type);

        /// <summary>
        /// Makes a testable default expression for the given type.
        /// </summary>
        /// <param name="type">The type to make a default expression for.</param>
        /// <returns>The default expression.</returns>
        public static Expression For(Type type)
        {
            // Although we can just use default there, putting null is a better choice because it
            // allows for a better readability, better testing (default -> null at compile time)
            // and we can avoid some surprise bugs with EF providers that don't support default.
            // (...If those even exist???)

            if (type.IsValueType)
            {
                return Expression.Default(type);
            }

            return Expression.Constant(null, type);
        }
    }
}