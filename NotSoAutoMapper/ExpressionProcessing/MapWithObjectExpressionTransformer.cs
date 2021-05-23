using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal class MapWithObjectExpressionTransformer : IMethodExpressionTransformer
    {
        public Expression Transform(MethodCallExpression expression) 
            => MapperInliningOperations.InlineObject(expression.Arguments[0], expression.Arguments[1]);
    }
}