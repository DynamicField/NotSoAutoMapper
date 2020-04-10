using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HandmadeMapper.Extensions.Ioc.Base.Tests
{
    [TestClass]
    public class IocContainerResolverTests
    {
        [TestMethod]
        public void ResolveMapper_BuildsCorrectType()
        {
            Type actualType = null;
            var resolver = new TestIocContainerMapperResolver(t =>
            {
                actualType = t;
                return null;
            });
            Expression<Action> methodCallContainer = () => Mapper.Include<object, int>("whatever");
            var methodCall = (MethodCallExpression) methodCallContainer.Body;

            resolver.ResolveMapper(methodCall);

            Assert.AreEqual(typeof(IMapper<object, int>), actualType);
        }

        private class TestIocContainerMapperResolver : IocContainerMapperResolver
        {
            private readonly Func<Type, IMapperExpressionProvider> _onGetService;

            public TestIocContainerMapperResolver(Func<Type, IMapperExpressionProvider> onGetService)
            {
                _onGetService = onGetService;
            }

            protected sealed override IMapperExpressionProvider GetService(Type type)
            {
                return _onGetService(type);
            }
        }
    }
}