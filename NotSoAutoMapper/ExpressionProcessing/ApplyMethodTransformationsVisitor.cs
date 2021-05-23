using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal sealed class ApplyMethodTransformationsVisitor : ExpressionVisitor
    {
        private static readonly ConcurrentDictionary<MethodInfo, IMethodExpressionTransformer?> s_methodTransformerCache = new();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var transformer = s_methodTransformerCache.GetOrAdd(node.Method, ProvideExpressionTransformer);
            if (transformer is null)
            {
                return base.VisitMethodCall(node);
            }

            return transformer.Transform(node);
        }

        private IMethodExpressionTransformer? ProvideExpressionTransformer(MethodInfo method)
        {
            var attribute = method.GetCustomAttribute<TransformedUsingAttribute>();
            if (attribute is null)
            {
                return null;
            }

            object uncastedTransformer;
            try
            {
                // We use Activator here because compiling a LINQ expression and only using it once
                // would just take more time than anything else. Also I'm not really fussed to compile a LINQ expression rn.
                uncastedTransformer = Activator.CreateInstance(attribute.MethodExpressionTransformerType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    $"Failed to instantiate {nameof(TransformedUsingAttribute)}'s transformer of type " +
                    $"{attribute.MethodExpressionTransformerType}.", e);
            }

            if (uncastedTransformer is not IMethodExpressionTransformer transformer)
            {
                throw new InvalidOperationException(
                    $"The type {attribute.MethodExpressionTransformerType} specified in a {nameof(TransformedUsingAttribute)} " +
                    $"does not implement {nameof(IMethodExpressionTransformer)}.");
            }

            return transformer;
        }
    }
}
