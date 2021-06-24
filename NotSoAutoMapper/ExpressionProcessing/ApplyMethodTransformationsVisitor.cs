using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal sealed class ApplyMethodTransformationsVisitor : ExpressionVisitor
    {
        private static readonly ConcurrentDictionary<MethodInfo, IMethodExpressionTransformer?>
            s_methodTransformersCache = new();

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var transformer = s_methodTransformersCache.GetOrAdd(node.Method, ProvideExpressionTransformer);
            return transformer is null ? base.VisitMethodCall(node) : transformer.Transform(node);
        }

        private static IMethodExpressionTransformer? ProvideExpressionTransformer(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes<TransformedUsingAttribute>().ToArray();
            switch (attributes.Length)
            {
                case 0:
                    return null;
                case 1:
                {
                    var attribute = attributes[0];
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
                default:
                    throw new InvalidOperationException(
                        $"Multiple {nameof(TransformedUsingAttribute)} attributes are present on the method {method}." +
                        " This scenario is not supported at the moment.");
            }
        }
    }
}