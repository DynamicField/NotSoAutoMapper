using System;
using DryIoc;
using NotSoAutoMapper.Extensions.Ioc.Base;

namespace NotSoAutoMapper.Extensions.Ioc.DryIoc
{
    /// <summary>
    ///     Provides extension methods to use DryIoc with NotSoAutoMapper.
    /// </summary>
    public static class NotSoAutoMapperDryIocContainerExtensions
    {
        private const string RegisterMapperFromServiceKey = "_registerMappersFrom";

        /// <summary>
        ///     Adds basic NotSoAutoMapper functionality.
        /// </summary>
        /// <param name="registrator">The registrator.</param>
        public static void RegisterNotSoAutoMapper(this IRegistrator registrator) => NotSoAutoMapperIocContainerUtilities.AddNotSoAutoMapper(GetRegisterSingletonService(registrator));

        /// <summary>
        ///     Register the specified <paramref name="mapper" />.
        /// </summary>
        /// <typeparam name="TSource">The source type of the mapper.</typeparam>
        /// <typeparam name="TTarget">The target type of the mapper.</typeparam>
        /// <param name="registrator">The registrator.</param>
        /// <param name="mapper"></param>
        /// <param name="ifAlreadyRegistered"></param>
        /// <param name="setup"></param>
        /// <param name="serviceKey"></param>
        public static void RegisterMapper<TSource, TTarget>(this IRegistrator registrator,
            IMapper<TSource, TTarget> mapper, IfAlreadyRegistered? ifAlreadyRegistered = null, Setup? setup = null,
            object? serviceKey = null) => registrator.RegisterInstance(mapper, ifAlreadyRegistered, setup!, serviceKey);

        /// <summary>
        ///     Registers a mapper of type <typeparamref name="T" />, with all <see cref="IMapper{TInput,TResult}" /> this class
        ///     implements.
        /// </summary>
        /// <param name="registrator">The registrator.</param>
        /// <param name="made"></param>
        /// <param name="setup"></param>
        /// <param name="ifAlreadyRegistered"></param>
        /// <param name="serviceKey"></param>
        public static void RegisterMapper<T>(this IRegistrator registrator, Made? made = null,
            Setup? setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object? serviceKey = null)
        {
            NotSoAutoMapperIocContainerUtilities.AddMapper(typeof(T),
                GetRegisterSingletonService(registrator, made, setup, ifAlreadyRegistered, serviceKey));
        }

        /// <inheritdoc cref="NotSoAutoMapperIocContainerUtilities.AddMapper" />
        /// <param name="registrator">The registrator.</param>
        /// <param name="mapperType">The type of the mapper to register.</param>
        /// <param name="made"></param>
        /// <param name="setup"></param>
        /// <param name="ifAlreadyRegistered"></param>
        /// <param name="serviceKey"></param>
        public static void RegisterMapper(this IRegistrator registrator, Type mapperType, Made? made = null,
            Setup? setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object? serviceKey = null)
        {
            NotSoAutoMapperIocContainerUtilities.AddMapper(mapperType,
                GetRegisterSingletonService(registrator, made, setup, ifAlreadyRegistered, serviceKey));
        }

        /// <summary>
        ///     <para>
        ///         Adds the resolver to create <c>Mapper</c> objects using <c>RegisterMappersFrom</c>.
        ///     </para>
        ///     <para>
        ///         You can use it on your container like that:
        ///         <code>
        /// var container = new Container(rules =&gt; rules.WithRegisterMappersFromResolver());
        /// </code>
        ///     </para>
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <returns>The same rules.</returns>
        public static Rules WithRegisterMappersFromResolver(this Rules rules)
        {
            if (rules is null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            return rules.WithConcreteTypeDynamicRegistrations(
                (_, serviceKey) => RegisterMapperFromServiceKey.Equals(serviceKey),
                Reuse.Singleton);
        }


        /// <inheritdoc cref="NotSoAutoMapperIocContainerUtilities.AddMappersFrom" />
        /// <typeparam name="T">The type containing the static methods.</typeparam>
        /// <param name="registrator">The registrator.</param>
        public static void RegisterMappersFrom<T>(this IRegistrator registrator) => registrator.RegisterMappersFrom(typeof(T));

        /// <inheritdoc cref="NotSoAutoMapperIocContainerUtilities.AddMappersFrom" />
        /// <param name="registrator">The registrator.</param>
        /// <param name="type">The type containing the static methods.</param>
        public static void RegisterMappersFrom(this IRegistrator registrator, Type type)
        {
            Made.Of(() => 5);

            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(type, (method, descriptor, getter) => {
                registrator.RegisterDelegate(descriptor.ServiceType, resolver => {
                    var expression = getter(resolver.Resolve!);
                    try
                    {
                        return resolver.Resolve(descriptor.ImplementationType, new object[] { expression },
                            serviceKey: RegisterMapperFromServiceKey);
                    }
                    catch (ContainerException e)
                    {
                        throw new ContainerException(e.Error,
                            "Couldn't resolve the mapper, you probably forgot to call rules.WithRegisterMappersFromResolver(). \n" +
                            e.Message, e);
                    }
                });
            });
        }

        private static RegisterSingletonService GetRegisterSingletonService(IRegistrator registrator, Made? made = null,
            Setup? setup = null, IfAlreadyRegistered? ifAlreadyRegistered = null, object? serviceKey = null)
        {
            return (s, i) =>
                registrator.Register(s, i, Reuse.Singleton, made!, setup!, ifAlreadyRegistered, serviceKey!);
        }
    }
}