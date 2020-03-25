using System;
using HandmadeMapper.Extensions.Ioc.Base;
using Microsoft.Extensions.DependencyInjection;

namespace HandmadeMapper.Extensions.Ioc.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering mappers in a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class HandmadeMapperServiceProviderExtensions
    {
        /// <summary>
        /// Adds HandmadeMapper functionality in the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to register with.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddHandmadeMapper(this IServiceCollection services)
        {
            HandmadeMapperIocContainerUtilities.AddHandmadeMapper(GetRegisterSingletonService(services), typeof(IocContainerMapperResolver));
            return services;
        }

        /// <inheritdoc cref="AddMappersFrom"/>
        /// <typeparam name="T">The type containing the static methods.</typeparam>
        public static IServiceCollection AddMappersFrom<T>(this IServiceCollection services)
        {
            return services.AddMappersFrom(typeof(T));
        }

        /// <inheritdoc cref="HandmadeMapperIocContainerUtilities.AddMappersFrom"/>
        /// <param name="services">The <see cref="IServiceCollection" /> to register with.</param>
        /// <param name="type">The type containing the static methods.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMappersFrom(this IServiceCollection services, Type type)
        {
            HandmadeMapperIocContainerUtilities.AddMappersFrom(type, (method, types, methodGetter) =>
            {
                services.AddSingleton(types.ServiceType, provider =>
                {
                    var expression = methodGetter(provider.GetRequiredService!);
                    return ActivatorUtilities.CreateInstance(provider, types.ImplementationType, expression);
                });
            });
            return services;
        }

        /// <summary>
        ///     Registers the <paramref name="mapper"/> to the specified
        ///     <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to register with.</param>
        /// <param name="mapper">The mapper.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMapper<TInput, TResult>(this IServiceCollection services, IMapper<TInput, TResult> mapper)
        {
            return services.AddSingleton(mapper);
        }

        /// <summary>
        ///     Registers the mapper of the specified type <typeparamref name="T" /> to the specified
        ///     <see cref="IServiceCollection" />.
        /// </summary>
        /// <typeparam name="T">The type of the mapper.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection" /> to register with.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMapper<T>(this IServiceCollection services)
        {
            HandmadeMapperIocContainerUtilities.AddMapper(typeof(T), GetRegisterSingletonService(services));
            return services;
        }

        /// <summary>
        ///     Registers the mapper of the specified <paramref name="mapperType" /> to the specified
        ///     <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to register with.</param>
        /// <param name="mapperType">The type of the mapper.</param>
        /// <returns>The original <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMapper(this IServiceCollection services, Type mapperType)
        {
            HandmadeMapperIocContainerUtilities.AddMapper(mapperType, GetRegisterSingletonService(services));
            return services;
        }

        private static RegisterSingletonService GetRegisterSingletonService(IServiceCollection services)
        {
            return (serviceType, implementationType) => services.AddSingleton(serviceType, implementationType);
        }
    }
}