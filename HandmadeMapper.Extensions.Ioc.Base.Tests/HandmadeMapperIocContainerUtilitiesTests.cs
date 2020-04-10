using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HandmadeMapper.ExpressionProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace HandmadeMapper.Extensions.Ioc.Base.Tests
{
    [TestClass]
    public class HandmadeMapperIocContainerUtilitiesTests
    {
        [TestMethod]
        public void AddHandmadeMapper_AddsRequiredServices()
        {
            var container = new SimpleIocContainer();

            HandmadeMapperIocContainerUtilities.AddHandmadeMapper(container.Register);

            CollectionAssert.Contains(container.Registrations,
                (typeof(IExpressionTransformer), typeof(IncludeExpressionTransformer)));
        }

        [TestMethod]
        public void AddHandmadeMapper_AddsGivenMapperResolver()
        {
            var container = new SimpleIocContainer();
            var mapperResolverType = typeof(HandmadeMapperIocContainerUtilitiesTests);

            HandmadeMapperIocContainerUtilities.AddHandmadeMapper(container.Register, mapperResolverType);

            CollectionAssert.Contains(container.Registrations, (typeof(IMapperResolver), mapperResolverType));
        }

        [TestMethod]
        public void AddMapper_AddsAllInterfaces()
        {
            var container = new SimpleIocContainer();
            var imapperTypes = new[] {typeof(IMapper<object, int>), typeof(IMapper<double, int>)};
            var mapper = Substitute.For(imapperTypes, Array.Empty<object>());

            HandmadeMapperIocContainerUtilities.AddMapper(mapper.GetType(), container.Register);

            var expectedRegistrations = imapperTypes.Select(x => (x, mapper.GetType())).ToList();
            CollectionAssert.AreEquivalent(expectedRegistrations, container.Registrations);
        }

        [TestMethod]
        public void AddMapper_ThrowsWithNoInterfaces()
        {
            var container = new SimpleIocContainer();

            Assert.ThrowsException<InvalidOperationException>(
                () => HandmadeMapperIocContainerUtilities.AddMapper(typeof(object), container.Register));
        }

        [TestMethod]
        public void AddMappersFrom_AddsValidMappers()
        {
            var methods = new List<MethodInfo>();

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass),
                (m, _, __) => methods.Add(m));

            CollectionAssert.Contains(methods, AddMappersFromTestClass.ValidMapperSimpleMethodInfo);
            CollectionAssert.Contains(methods, AddMappersFromTestClass.ValidMapperWithDependenciesMethodInfo);
            CollectionAssert.Contains(methods, AddMappersFromTestClass.ValidLazyMapperMethodInfo);
        }

        [TestMethod]
        public void AddMappersFrom_DoesNotAddNotExpressionMethods()
        {
            var methods = new List<MethodInfo>();

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass),
                (m, _, __) => methods.Add(m));

            CollectionAssert.DoesNotContain(methods, AddMappersFromTestClass.InvalidMapperNotExpressionMethodInfo);
        }

        [TestMethod]
        public void AddMappersFrom_DoesNotAddExcludedMethods()
        {
            var methods = new List<MethodInfo>();

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass),
                (m, _, __) => methods.Add(m));

            CollectionAssert.DoesNotContain(methods, AddMappersFromTestClass.InvalidMapperExcludedMethodInfo);
        }

        [TestMethod]
        public void AddMappersFrom_ResolvesMethodsWithoutParameters()
        {
            IMapperOptions options = null;

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, _, getter) =>
            {
                if (m != AddMappersFromTestClass.ValidMapperSimpleMethodInfo) return;
                options = getter(__ =>
                {
                    Assert.Fail("There aren't any services to resolve.");
                    return null;
                });
            });

            Assert.AreSame(options.Expression, AddMappersFromTestClass.ValidMapperSimple());
        }

        [TestMethod]
        public void AddMappersFrom_ResolvesMethodsWithParameters()
        {
            IMapperOptions options = null;
            var dependency = new Mapper<object, int>(AddMappersFromTestClass.ValidMapperSimple());

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, _, getter) =>
            {
                if (m != AddMappersFromTestClass.ValidMapperWithDependenciesMethodInfo) return;
                options = getter(__ => dependency);
            });

            Assert.AreSame(dependency, AddMappersFromTestClass.ValidMapperWithDependenciesLastParameterValue);
            Assert.AreSame(options.Expression, AddMappersFromTestClass.ValidMapperWithDependencies(dependency));
        }

        [TestMethod]
        public void AddMappersFrom_HasCorrectRegistrationDescriptor()
        {
            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, descriptor, _) =>
            {
                void MustBe<TInput, TOutput>()
                {
                    Assert.AreEqual(typeof(IMapper<TInput, TOutput>), descriptor.ServiceType);
                    Assert.AreEqual(typeof(Mapper<TInput, TOutput>), descriptor.ImplementationType);
                }

                if (m == AddMappersFromTestClass.ValidMapperSimpleMethodInfo)
                {
                    MustBe<object, int>();
                }
                else if (m == AddMappersFromTestClass.ValidMapperWithDependenciesMethodInfo)
                {
                    MustBe<object, double>();
                }
                else if (m == AddMappersFromTestClass.ValidLazyMapperMethodInfo)
                {
                    MustBe<object, object>();
                }
                else if (m == AddMappersFromTestClass.ValidIMappersOptionsMethodInfo)
                {
                    MustBe<object, decimal>();
                }
                else
                {
                    Assert.Inconclusive("Unknown method: " + m);
                }
            });
        }

        [TestMethod]
        public void AddMappersFrom_LazyMapperIsLazy()
        {
            IMapperOptions options = null;

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, _, getter) =>
            {
                if (m != AddMappersFromTestClass.ValidLazyMapperMethodInfo) return;
                options = getter(__ => null);
            });

            Assert.IsTrue(options.IsLazy);
        }

        [TestMethod]
        public void AddMappersFrom_ValidIMapperGivesMapperOption()
        {
            IMapperOptions options = null;

            HandmadeMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, _, getter) =>
            {
                if (m != AddMappersFromTestClass.ValidIMappersOptionsMethodInfo) return;
                options = getter(__ => null);
            });

            Assert.AreSame(AddMappersFromTestClass.ValidIMapperOptions(), options);
        }

        private class SimpleIocContainer
        {
            public List<(Type serviceType, Type implementationType)> Registrations { get; } =
                new List<(Type serviceType, Type implementationType)>();

            public void Register(Type serviceType, Type implementationType)
            {
                Registrations.Add((serviceType, implementationType));
            }
        }
    }

    public static class AddMappersFromTestClass
    {
        private static readonly Expression<Func<object, int>> ValidMapperSimpleExpression = o => 1;

        public static readonly MethodInfo ValidMapperSimpleMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(ValidMapperSimple));

        private static readonly Expression<Func<object, double>> ValidMapperWithDependenciesExpression = o => 1.1;

        public static readonly MethodInfo ValidMapperWithDependenciesMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(ValidMapperWithDependencies));

        public static readonly MethodInfo InvalidMapperNotExpressionMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(InvalidMapperNotExpression));

        public static readonly MethodInfo InvalidMapperExcludedMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(InvalidMapperExcluded));

        public static readonly MethodInfo ValidLazyMapperMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(ValidLazyMapper));

        public static readonly MethodInfo ValidIMappersOptionsMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(ValidIMapperOptions));

        private static readonly MapperOptions<object, decimal> ValidIMapperOptionsMapperOptions 
            = new MapperOptions<object, decimal>(x => 1.9m);


        public static IMapper<object, int> ValidMapperWithDependenciesLastParameterValue { get; private set; }

        public static Expression<Func<object, int>> ValidMapperSimple()
        {
            return ValidMapperSimpleExpression;
        }

        public static Expression<Func<object, double>> ValidMapperWithDependencies(IMapper<object, int> otherMapper)
        {
            ValidMapperWithDependenciesLastParameterValue = otherMapper;
            return ValidMapperWithDependenciesExpression;
        }

        public static void InvalidMapperNotExpression()
        {
            // Empty
        }


        [ExcludeMapper]
        public static Expression<Func<object, object>> InvalidMapperExcluded()
        {
            return o => o;
        }

        [Lazy]
        public static Expression<Func<object, object>> ValidLazyMapper()
        {
            return x => x;
        }

        public static IMapperOptions<object, decimal> ValidIMapperOptions()
        {
            return ValidIMapperOptionsMapperOptions;
        }
    }
}