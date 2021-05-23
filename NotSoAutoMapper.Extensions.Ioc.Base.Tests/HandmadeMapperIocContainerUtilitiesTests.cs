using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace NotSoAutoMapper.Extensions.Ioc.Base.Tests
{
    [TestClass]
    public class NotSoAutoMapperIocContainerUtilitiesTests
    {
        [TestMethod]
        public void AddNotSoAutoMapper_AddsRequiredServices()
        {
            var container = new SimpleIocContainer();

            NotSoAutoMapperIocContainerUtilities.AddNotSoAutoMapper(container.Register);

            // Yay!
        }

        [TestMethod]
        public void AddMapper_AddsAllInterfaces()
        {
            var container = new SimpleIocContainer();
            var imapperTypes = new[] {typeof(IMapper<object, int>), typeof(IMapper<double, int>)};
            var mapper = Substitute.For(imapperTypes, Array.Empty<object>());

            NotSoAutoMapperIocContainerUtilities.AddMapper(mapper.GetType(), container.Register);

            var expectedRegistrations = imapperTypes.Select(x => (x, mapper.GetType())).ToList();
            CollectionAssert.AreEquivalent(expectedRegistrations, container.Registrations);
        }

        [TestMethod]
        public void AddMapper_ThrowsWithNoInterfaces()
        {
            var container = new SimpleIocContainer();

            Assert.ThrowsException<InvalidOperationException>(
                () => NotSoAutoMapperIocContainerUtilities.AddMapper(typeof(object), container.Register));
        }

        [TestMethod]
        public void AddMappersFrom_AddsValidMappers()
        {
            var methods = new List<MethodInfo>();

            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass),
                (m, _, __) => methods.Add(m));

            CollectionAssert.Contains(methods, AddMappersFromTestClass.ValidMapperSimpleMethodInfo);
            CollectionAssert.Contains(methods, AddMappersFromTestClass.ValidMapperWithDependenciesMethodInfo);
        }

        [TestMethod]
        public void AddMappersFrom_DoesNotAddNotExpressionMethods()
        {
            var methods = new List<MethodInfo>();

            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass),
                (m, _, __) => methods.Add(m));

            CollectionAssert.DoesNotContain(methods, AddMappersFromTestClass.InvalidMapperNotExpressionMethodInfo);
        }

        [TestMethod]
        public void AddMappersFrom_DoesNotAddExcludedMethods()
        {
            var methods = new List<MethodInfo>();

            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass),
                (m, _, __) => methods.Add(m));

            CollectionAssert.DoesNotContain(methods, AddMappersFromTestClass.InvalidMapperExcludedMethodInfo);
        }

        [TestMethod]
        public void AddMappersFrom_ResolvesMethodsWithoutParameters()
        {
            LambdaExpression expression = null;

            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, _, getter) =>
            {
                if (m != AddMappersFromTestClass.ValidMapperSimpleMethodInfo)
                {
                    return;
                }

                expression = getter(__ =>
                {
                    Assert.Fail("There aren't any services to resolve.");
                    return null;
                });
            });

            Assert.AreSame(expression, AddMappersFromTestClass.ValidMapperSimple());
        }

        [TestMethod]
        public void AddMappersFrom_ResolvesMethodsWithParameters()
        {
            LambdaExpression expression = null;
            var dependency = new Mapper<object, int>(AddMappersFromTestClass.ValidMapperSimple());

            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, _, getter) =>
            {
                if (m != AddMappersFromTestClass.ValidMapperWithDependenciesMethodInfo)
                {
                    return;
                }

                expression = getter(__ => dependency);
            });

            Assert.AreSame(dependency, AddMappersFromTestClass.ValidMapperWithDependenciesLastParameterValue);
            Assert.AreSame(expression, AddMappersFromTestClass.ValidMapperWithDependencies(dependency));
        }

        [TestMethod]
        public void AddMappersFrom_HasCorrectRegistrationDescriptor()
        {
            NotSoAutoMapperIocContainerUtilities.AddMappersFrom(typeof(AddMappersFromTestClass), (m, descriptor, _) =>
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
                else
                {
                    Assert.Inconclusive("Unknown method: " + m);
                }
            });
        }

        private class SimpleIocContainer
        {
            public List<(Type serviceType, Type implementationType)> Registrations { get; } =
                new List<(Type serviceType, Type implementationType)>();

            public void Register(Type serviceType, Type implementationType) => Registrations.Add((serviceType, implementationType));
        }
    }

    public static class AddMappersFromTestClass
    {
        private static readonly Expression<Func<object, int>> s_validMapperSimpleExpression = o => 1;

        public static readonly MethodInfo ValidMapperSimpleMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(ValidMapperSimple));

        private static readonly Expression<Func<object, double>> s_validMapperWithDependenciesExpression = o => 1.1;

        public static readonly MethodInfo ValidMapperWithDependenciesMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(ValidMapperWithDependencies));

        public static readonly MethodInfo InvalidMapperNotExpressionMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(InvalidMapperNotExpression));

        public static readonly MethodInfo InvalidMapperExcludedMethodInfo =
            typeof(AddMappersFromTestClass).GetMethod(nameof(InvalidMapperExcluded));

        public static IMapper<object, int> ValidMapperWithDependenciesLastParameterValue { get; private set; }

        public static Expression<Func<object, int>> ValidMapperSimple() => s_validMapperSimpleExpression;

        public static Expression<Func<object, double>> ValidMapperWithDependencies(IMapper<object, int> otherMapper)
        {
            ValidMapperWithDependenciesLastParameterValue = otherMapper;
            return s_validMapperWithDependenciesExpression;
        }

        public static void InvalidMapperNotExpression()
        {
            // Empty
        }


        [ExcludeMapper]
        public static Expression<Func<object, object>> InvalidMapperExcluded() => o => o;
    }
}