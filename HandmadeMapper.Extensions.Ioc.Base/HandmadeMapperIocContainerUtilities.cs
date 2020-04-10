using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HandmadeMapper.ExpressionProcessing;

namespace HandmadeMapper.Extensions.Ioc.Base
{
    /// <summary>
    ///     A delegate to get the actual <see cref="MapperOptions{TSource,TTarget}" /> value of a mapper static method.
    /// </summary>
    /// <param name="serviceResolver">A function to resolve a service, given its type.</param>
    /// <returns>The actual <see cref="MapperOptions{TSource,TTarget}" /> returned from the method.</returns>
    public delegate IMapperOptions StaticMapperMethodGetter(Func<Type, object> serviceResolver);

    /// <summary>
    ///     <para>
    ///         A delegate that registers a singleton service, with the specified <paramref name="serviceType" /> and
    ///         <paramref name="implementationType" />.
    ///     </para>
    ///     <para>
    ///         In most IoC containers, this should be implemented like that:
    ///         <c>services.AddSingleton(serviceType, implementationType)</c>.
    ///     </para>
    /// </summary>
    /// <param name="serviceType">
    ///     The service type (the type to request to get an <paramref name="implementationType" />
    ///     instance).
    /// </param>
    /// <param name="implementationType">
    ///     The implementation type (an instance of this type will be get
    ///     when requesting an object of type <paramref name="implementationType" />).
    /// </param>
    public delegate void RegisterSingletonService(Type serviceType, Type implementationType);

    /// <summary>
    ///     Provides base functionality for registering mappers in IoC containers.
    /// </summary>
    public static class HandmadeMapperIocContainerUtilities
    {
        /// <summary>
        ///     Adds HandmadeMapper functionality.
        /// </summary>
        /// <param name="registerSingletonService">The delegate to use to register a singleton in the IoC container.</param>
        /// <param name="mapperResolverType">The <see cref="IMapperResolver" /> to use for resolving services.</param>
        public static void AddHandmadeMapper(RegisterSingletonService registerSingletonService,
            Type? mapperResolverType = null)
        {
            if (registerSingletonService is null)
                throw new ArgumentNullException(nameof(registerSingletonService));

            foreach (var expressionTransformerType in Mapper.DefaultExpressionTransformers.Select(x => x.GetType())
                .Distinct()) registerSingletonService(typeof(IExpressionTransformer), expressionTransformerType);

            if (mapperResolverType != null)
                registerSingletonService(typeof(IMapperResolver), mapperResolverType);
        }

        /// <summary>
        ///     Registers the mapper of the specified <paramref name="mapperType" />. It must implement
        ///     <see cref="IMapper{TInput,TResult}" />.
        ///     If it implements multiple generic versions of it, all of them are registered.
        /// </summary>
        /// <param name="mapperType">The type of the mapper.</param>
        /// <param name="registerSingletonService">The delegate to use to register a singleton in the IoC container.</param>
        /// <exception cref="InvalidOperationException">
        ///     When the mapper does not implement <see cref="IMapper{TInput,TResult}" />
        /// </exception>
        public static void AddMapper(Type mapperType, RegisterSingletonService registerSingletonService)
        {
            if (mapperType is null)
                throw new ArgumentNullException(nameof(mapperType));

            var mapperInterfaces =
                mapperType.FindInterfaces((t, _) => IsGenericTypeFromUnbound(t, typeof(IMapper<,>)), null);

            if (!mapperInterfaces.Any())
                throw new InvalidOperationException($"The type {mapperType} does not implement IMapper.");

            foreach (var mapperInterface in mapperInterfaces) registerSingletonService(mapperInterface, mapperType);
        }

        /// <summary>
        ///     <para>
        ///         Registers all mappers from static methods in the specified
        ///         <paramref name="type" />.
        ///     </para>
        ///     <para>
        ///         This method finds all static methods in <paramref name="type" /> returning a
        ///         <see cref="Expression{TDelegate}" /> or a <see cref="MapperOptions{TSource,TTarget}" />,
        ///         whose <c>TDelegate</c> is <see cref="Func{T, TResult}" />,
        ///         except those marked with <see cref="ExcludeMapperAttribute" />.
        ///     </para>
        ///     <para>
        ///         Then, for each method, it registers a <see cref="IMapper{TInput,TResult}" /> service:
        ///         <list type="bullet">
        ///             <item>
        ///                 whose <c>TInput</c> is the <b>first generic argument</b> (<c>T</c>) of the earlier
        ///                 <see cref="Func{T, TResult}" />
        ///             </item>
        ///             <item>
        ///                 whose <c>TResult</c> is the <b>second generic argument</b> (<c>TResult</c>) of the earlier
        ///                 <see cref="Func{T, TResult}" />
        ///             </item>
        ///             <item>
        ///                 when returning an <see cref="Expression{TDelegate}" />, creates an <see cref="IMapperOptions" /> with
        ///                 the expression, or <br />
        ///                 when returning an <see cref="MapperOptions{TSource,TTarget}" />, uses the returned value.
        ///             </item>
        ///             <item>
        ///                 and <b>resolving the method parameters</b> (if any), using the IoC container.
        ///             </item>
        ///             <item>
        ///                 being an instance of <see cref="Mapper{TInput,TResult}" /> with the <see cref="IMapperOptions" />
        ///                 created earlier.
        ///             </item>
        ///         </list>
        ///     </para>
        /// </summary>
        /// <example>
        ///     Let's say we have the following classes:
        ///     <code>
        /// <![CDATA[
        /// public class Cat
        /// {
        ///     // Properties...
        ///     public static Expression<Func<Cat, CatDto>> GetCatDtoMap() =>
        ///         x => new CatDto
        ///         {
        ///             Id = x.Id,
        ///             Name = x.Name,
        ///             Weight = x.Weight
        ///         };
        /// }
        /// public class Thing
        /// {
        ///     // Properties...
        ///     public static Expression<Func<Thing, ThingDto>> GetThingDtoMap(IMapper<Cat, CatDto> catMapper) =>
        ///         x => new ThingDto
        ///         {
        ///             Id = x.Id,
        ///             Name = x.Name,
        ///             Cat = EfMapper.Include(x.Cat, catMapper)
        ///         };
        /// }]]>
        /// </code>
        ///     And then, in the services:
        ///     <code><![CDATA[
        /// services.AddMappersFrom<Cat>();
        /// services.AddMappersFrom<Thing>();
        /// ]]>
        /// </code>
        ///     Now, <c><![CDATA[IMapper<Cat, CatDto>]]></c> and <c><![CDATA[IMapper<Thing, ThingDto>]]></c> are registered.
        ///     We may now use them:
        ///     <code>
        /// <![CDATA[
        /// public class SomeStuff
        /// {
        ///     private IMapper<Cat, CatDto> _catMapper;
        /// 
        ///     public SomeStuff(IMapper<Cat, CatDto> catMapper)
        ///     {
        ///         _catMapper = catMapper; // Injected!
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        /// <param name="type">The type containing the static methods.</param>
        /// <param name="registerAction">The action used to register each method.</param>
        /// <seealso cref="LazyAttribute" />
        public static void AddMappersFrom(Type type,
            Action<MethodInfo, RegistrationDescriptor, StaticMapperMethodGetter> registerAction)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var maybeCompatibleMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.GetCustomAttribute<ExcludeMapperAttribute>() == null)
                .Select(method => new
                {
                    Method = method,
                    MapperTypes = FindMapperTypesFromReturnType(method)
                });

            foreach (var item in maybeCompatibleMethods)
            {
                if (item.MapperTypes is null) continue; // Now it's compatible

                var method = item.Method;
                var (imapperType, mapperType) = item.MapperTypes.Value;


                IMapperOptions MethodGetter(Func<Type, object> resolver)
                {
                    var resolvedParameterDependencies = method.GetParameters()
                        .Select(t => resolver(t.ParameterType))
                        .ToArray();

                    IMapperOptions options;

                    var result = method.Invoke(null, resolvedParameterDependencies);

                    if (result is null) throw new InvalidOperationException($"{method.Name} returned null.");

                    // It may be either an Expression or MapperOptions.
                    if (IsGenericTypeFromUnbound(method.ReturnType, typeof(Expression<>)))
                    {
                        // Then let's create an option
                        var mapperOptionsType = typeof(MapperOptions<,>).MakeGenericType(
                            mapperType.GenericTypeArguments[0],
                            mapperType.GenericTypeArguments[1]);

                        options = (IMapperOptions) Activator.CreateInstance(mapperOptionsType, result, false);
                        options.Expression = (Expression) result;
                    }
                    else // It's MapperOptions
                    {
                        options = (IMapperOptions) result;
                    }

                    if (method.GetCustomAttribute<LazyAttribute>() != null)
                        options.IsLazy = true;

                    return options;
                }

                registerAction(method, new RegistrationDescriptor(imapperType, mapperType), MethodGetter);
            }
        }

        private static (Type imapperType, Type mapperType)? FindMapperTypesFromReturnType(MethodInfo method)
        {
            var returnType = method.ReturnType;

            Type funcType;
            if (IsGenericTypeFromUnbound(returnType, typeof(Expression<>)))
            {
                funcType = returnType.GenericTypeArguments[0];
                if (!IsGenericTypeFromUnbound(funcType, typeof(Func<,>))) return null;
            }
            else if (IsGenericTypeFromUnbound(returnType, typeof(MapperOptions<,>)) ||
                     IsGenericTypeFromUnbound(returnType, typeof(IMapperOptions<,>)))
            {
                funcType = returnType; // MapperOptions<,> has two generic args.
            }
            else
            {
                return null;
            }

            var source = funcType.GenericTypeArguments[0];
            var target = funcType.GenericTypeArguments[1];

            var imapperType = typeof(IMapper<,>).MakeGenericType(source, target);
            var mapperType = typeof(Mapper<,>).MakeGenericType(source, target);

            return (imapperType, mapperType);
        }

        private static bool IsGenericTypeFromUnbound(Type typeA, Type unboundTypeB)
        {
            return typeA.IsGenericType && typeA.GetGenericTypeDefinition() == unboundTypeB;
        }
    }

    /// <summary>
    ///     Describes a service/implementation registration.
    /// </summary>
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types",
        Justification = "This struct is not used for comparisons.")]
    public readonly struct RegistrationDescriptor
    {
        /// <summary>
        ///     Creates a <see cref="RegistrationDescriptor" /> with the specified types.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        internal RegistrationDescriptor(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        /// <summary>
        ///     The service type.
        /// </summary>
        public Type ServiceType { get; }

        /// <summary>
        ///     The implementation type.
        /// </summary>
        public Type ImplementationType { get; }
    }
}