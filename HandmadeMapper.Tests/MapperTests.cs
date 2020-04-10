using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HandmadeMapper.ExpressionProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HandmadeMapper.Tests
{
    [TestClass]
    public class MapperTests
    {
        [TestMethod]
        public void UseExpression_AlreadyHavingAnException_Throws()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new ShouldThrowExceptionTestMapper());
        }

        [TestMethod]
        public void ExpressionTransformers_AreUsed_Eager()
        {
            var success = false;
            var processor = new TestExpressionTransformer(() => success = true);
            _ = new Mapper<object, object>(x => x, new[] {processor});

            // Normally, the expression should already have been processed.

            Assert.IsTrue(success, "success is false.");
        }

        [TestMethod]
        public void ExpressionTransformers_AreNotUsed_Lazy()
        {
            var used = false;
            var processor = new TestExpressionTransformer(() => used = true);

            _ = new Mapper<object, object>(MapperOptionsBuilder.Create<object, object>(x => x).AsLazy().Build(),
                new[] {processor});

            Assert.IsFalse(used, "used is true.");
        }

        [TestMethod]
        public void ExpressionTransformers_AreUsedUponRequest_Lazy()
        {
            var used = false;
            var processor = new TestExpressionTransformer(() => used = true);
            var mapper =
                new Mapper<object, object>(MapperOptionsBuilder.Create<object, object>(x => x).AsLazy().Build(),
                    new[] {processor});

            _ = mapper.Expression;

            Assert.IsTrue(used, "used is false.");
        }

        [TestMethod]
        public void Map_MapsObjectFromExpression()
        {
            const int number = 1;
            var mapper = new Mapper<int, int>(x => x + 5);

            var result = mapper.Map(number);

            Assert.AreEqual(number + 5, result);
        }

        [TestMethod]
        public void WithExpression_ChangesExpression()
        {
            var mapper = new Mapper<int, int>(x => x + 5);
            Expression<Func<int, int>> newExpression = x => x + 10;

            var newMapper = mapper.WithExpression(newExpression);

            Assert.AreSame(newExpression, newMapper.OriginalExpression);
        }

        [TestMethod]
        public void WithExpression_KeepsExpressionTransformers()
        {
            static IEnumerable<IExpressionTransformer> GetExpressionTransformers(object mapper)
            {
                var property = mapper.GetType().GetProperty("ExpressionTransformers",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                return (IEnumerable<IExpressionTransformer>) property!.GetValue(mapper)!;
            }

            var mapper = new Mapper<int, int>(x => x + 5);

            var newMapper = mapper.WithExpression(x => x + 10);

            CollectionAssert.AreEqual(GetExpressionTransformers(mapper).ToList(),
                GetExpressionTransformers(newMapper).ToList());
        }

        [TestMethod]
        public void IncludeOneParameter_CallThrows()
        {
            Assert.ThrowsException<InvalidOperationException>(() => Mapper.Include<int, int>(5));
        }

        [TestMethod]
        public void IncludeTwoParametersMapper_CallThrows()
        {
            Assert.ThrowsException<InvalidOperationException>(() => Mapper.Include(4, (IMapper<int, int>) null!));
        }

        [TestMethod]
        public void IncludeTwoParametersExpression_CallThrows()
        {
            Assert.ThrowsException<InvalidOperationException>(() =>
                Mapper.Include(4, (Expression<Func<int, int>>) null!));
        }

        private class ShouldThrowExceptionTestMapper : Mapper<object, object>
        {
            public ShouldThrowExceptionTestMapper(IEnumerable<IExpressionTransformer>? expressionTransformers = null) :
                base(x => new object(), expressionTransformers)
            {
                UseExpression(x => new object());
            }
        }

        private class TestExpressionTransformer : IExpressionTransformer
        {
            private readonly Action _onSuccess;

            public TestExpressionTransformer(Action onSuccess)
            {
                _onSuccess = onSuccess;
            }

            public Expression<T> Transform<T>(Expression<T> source, MappingContext context)
            {
                _onSuccess();
                return source;
            }
        }
    }
}