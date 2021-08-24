using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal class MapExpressionTransformer : IMethodExpressionTransformer
    {
        public Expression Transform(MethodCallExpression expression)
            => MapperInliningOperations.InlineObject(expression.Arguments[0], expression.Object!);
    }
}