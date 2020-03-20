using System.Linq.Expressions;

namespace HandmadeMapper.ExpressionProcessing
{
    /// <summary>
    /// Defines how to resolve the mapper from the <see cref="Mapper.Include{TInput,TResult}(TInput)" /> call.
    /// (e.g: <c>Mapper.Include(x.Thing)</c>, but not <c>Mapper.Include(x.Thing, someMapper)</c>).
    /// </summary>
    public interface IMapperResolver
    {
        /// <summary>
        /// Resolves the mapper from the <see cref="MethodCallExpression" /> of the method
        /// <see cref="Mapper.Include{TInput,TResult}(TInput)" />.
        /// </summary>
        /// <example>
        /// The method (<paramref name="expression" />) can be called, for example, like that:
        /// <code>Mapper.Include(x.Thing)</code>
        /// </example>
        /// <param name="expression">The method call, calling the <see cref="Mapper.Include{TInput,TResult}(TInput)" /> method.</param>
        /// <returns>The resolved mapper, or null if the mapper could not be resolved.</returns>
        IMapperExpressionProvider ResolveMapper(MethodCallExpression expression);
    }
}