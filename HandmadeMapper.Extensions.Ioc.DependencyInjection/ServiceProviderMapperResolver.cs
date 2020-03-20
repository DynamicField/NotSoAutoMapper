using System;
using HandmadeMapper.Extensions.Ioc.Base;

namespace HandmadeMapper.Extensions.Ioc.DependencyInjection
{
    /// <summary>
    /// Resolves mappers by getting the actual <see cref="IMapper{TInput,TResult}"/> service from a <see cref="IServiceProvider"/>.
    /// </summary>
    public sealed class ServiceProviderMapperResolver : IocContainerMapperResolver
    {
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Creates a <see cref="ServiceProviderMapperResolver"/> with the specified <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to use.</param>
        public ServiceProviderMapperResolver(IServiceProvider provider)
        {
            _provider = provider;
        }

        /// <inheritdoc />
        protected override IMapperExpressionProvider GetService(Type type)
        {
            return (IMapperExpressionProvider) _provider.GetService(type);
        }
    }
}