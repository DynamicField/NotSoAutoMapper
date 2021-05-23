using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    /// <summary>
    ///     Transforms <c>MapWith(mapper)</c> calls into the full <c>Select(x => new Thing { ... })</c> equivalent.
    /// </summary>
    internal class MapWithCollectionExpressionTransformer : IMethodExpressionTransformer
    {
        public Expression Transform(MethodCallExpression expression) 
            => MapperInliningOperations.InlineCollection(expression.Arguments[0], expression.Arguments[1]);
    }
}