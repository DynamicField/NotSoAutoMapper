using System;
using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal static class RetrieveExpressionValueExtensions
    {
        public static T? CompileAndGet<T>(this Expression expression)
        {
            var func = (Func<T?>) Expression.Lambda(expression).Compile();
            return func();
        }
    }
}
