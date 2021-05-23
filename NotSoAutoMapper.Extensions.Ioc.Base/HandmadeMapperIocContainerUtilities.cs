using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NotSoAutoMapper.Extensions.Ioc.Base
{
    /// <summary>
    ///     A delegate to get the actual <see cref="LambdaExpression" /> value of a mapper static method.
    /// </summary>
    /// <param name="serviceResolver">A function to resolve a service, given its type.</param>
    /// <returns>The actual <see cref="LambdaExpression" /> returned from the method.</returns>
    public delegate LambdaExpression StaticMapperMethodGetter(Func<Type, object> serviceResolver);

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
    public static class NotSoAutoMapperIocContainerUtilities
    {
        /// <summary>
        ///     Adds NotSoAutoMapper functionality.
        /// </summary>
        /// <param name="registerSingletonService">The delegate to use to register a singleton in the IoC container.</param>
        public static void AddNotSoAutoMapper(RegisterSingletonService registerSingletonService)
        {
            if (registerSingletonService is null)
            {
                throw new ArgumentNullException(nameof(registerSingletonService));
            }

            // Nothing for now.
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
            {
                throw new ArgumentNullException(nameof(mapperType));
            }

            var mapperInterfaces =
                mapperType.FindInterfaces((t, _) => IsGenericTypeFromUnbound(t, typeof(IMapper<,>)), null);

            if (!mapperInterfaces.Any())
            {
                throw new InvalidOperationException($"The type {mapperType} does not implement IMapper.");
            }

            foreach (var mapperInterface in mapperInterfaces)
            {
                registerSingletonService(mapperInterface, mapperType);
            }
        }

        /// <summary>
        ///     Registers all mappers from static methods in the specified
        ///     <paramref name="type" />, except those with the <see cref="ExcludeMapperAttribute"/>. 
        /// </summary>
        /// <remarks>
        ///     A method must be public and static, and must return an <c>Expression&lt;Func&lt;TSource, TTarget&gt;&gt;</c>.
        ///     Every parameter will be resolved using the IoC container. 
        ///     An <c>IMapper&lt;TSource, TTarget&gt;</c> will be registered using the expression returned by
        ///     the method. 
        /// </remarks>
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
        public static void AddMappersFrom(Type type,
            Action<MethodInfo, RegistrationDescriptor, StaticMapperMethodGetter> registerAction)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            var maybeCompatibleMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.GetCustomAttribute<ExcludeMapperAttribute>() == null)
                .Select(method => new {
                    Method = method,
                    MapperTypes = FindMapperTypesFromReturnType(method)
                });

            foreach (var item in maybeCompatibleMethods)
            {
                if (item.MapperTypes is null)
                {
                    // Now it's compatible
                    continue;
                }

                var method = item.Method;
                var (imapperType, mapperType) = item.MapperTypes.Value;

                LambdaExpression MethodGetter(Func<Type, object> resolver)
                {
                    var methodParameters = method.GetParameters();
                    var resolvedDependencies = new object[methodParameters.Length];

                    for (var i = 0; i < methodParameters.Length; i++)
                    {
                        var parameter = methodParameters[i];
                        try
                        {
                            resolvedDependencies[i] = resolver(parameter.ParameterType);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException(
                                $"Failed to resolve dependency for mapper method {method} in class " +
                                $"{method.DeclaringType} for parameter '{parameter.Name}'.", e);
                        }
                    }

                    var expression = (LambdaExpression?) method.Invoke(null, resolvedDependencies);


                    if (expression is null)
                    {
                        throw new InvalidOperationException($"{method.Name} returned null.");
                    }

                    return expression;
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
                if (!IsGenericTypeFromUnbound(funcType, typeof(Func<,>)))
                {
                    return null;
                }
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

        private static bool IsGenericTypeFromUnbound(Type typeA, Type unboundTypeB) => typeA.IsGenericType && typeA.GetGenericTypeDefinition() == unboundTypeB;
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