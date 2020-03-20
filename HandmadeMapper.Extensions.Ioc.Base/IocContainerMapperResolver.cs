using System;
using System.Linq.Expressions;
using HandmadeMapper.ExpressionProcessing;

namespace HandmadeMapper.Extensions.Ioc.Base
{
    /// <summary>
    /// The base class for resolving mappers by getting the actual <see cref="IMapper{TInput,TResult}"/> service from an IoC container.
    /// </summary>
    public abstract class IocContainerMapperResolver : IMapperResolver
    {
        /// <inheritdoc />
        public virtual IMapperExpressionProvider ResolveMapper(MethodCallExpression expression)
        {
            if (expression is null)
                throw new ArgumentNullException(nameof(expression));

            var source = expression.Method.GetGenericArguments()[0];
            var target = expression.Method.GetGenericArguments()[1];
            var mapperType = typeof(IMapper<,>).MakeGenericType(source, target);
            return GetService(mapperType);
        }

        /// <summary>
        /// Gets the specified service (of <paramref name="type"/>) from the IoC container.
        /// </summary>
        /// <param name="type">The type of the service to get.</param>
        /// <returns>A service of the specified <paramref name="type"/>.</returns>
        protected abstract IMapperExpressionProvider GetService(Type type);
    }
}