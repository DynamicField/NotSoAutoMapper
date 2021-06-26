using System;
using System.Linq.Expressions;
using System.Reflection;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal static class RetrieveExpressionValueExtensions
    {
        public static T? GetValue<T>(this Expression expression)
        {
            // Maintaining an expression cache is tricky, the Expression class doesn't implement .Equals!
            // Instead, we have some well-known common cases to boost the performance.
            // Reflection is much more faster than Expression compilation.
            
            if (expression is ConstantExpression constantExpression)
            {
                return (T?) constantExpression.Value;
            }

            if (expression is MemberExpression memberExpression)
            {
                var member = memberExpression.Member;
                switch (member)
                {
                    case PropertyInfo propertyInfo:
                        return (T?) propertyInfo.GetValue(memberExpression.Expression?.GetValue<object>());
                    case FieldInfo fieldInfo:
                        return (T?) fieldInfo.GetValue(memberExpression.Expression?.GetValue<object>());
                }
            }
            
            var func = (Func<T?>) Expression.Lambda(expression).Compile(true);
            return func();
        }
    }
}
