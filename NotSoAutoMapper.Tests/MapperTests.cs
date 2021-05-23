using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NotSoAutoMapper.ExpressionProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotSoAutoMapper.Tests
{
    [TestClass]
    public class MapperTests
    {
        [TestMethod]
        public void UseExpression_AlreadyHavingAnException_Throws() => Assert.ThrowsException<InvalidOperationException>(() => new ShouldThrowExceptionTestMapper());

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

        private class ShouldThrowExceptionTestMapper : Mapper<object, object>
        {
            public ShouldThrowExceptionTestMapper(IEnumerable<IMapperExpressionTransformer>? expressionTransformers = null) :
                base(x => new object(), expressionTransformers)
            {
                UseExpression(x => new object());
            }
        }

        private class TestExpressionTransformer : IMapperExpressionTransformer
        {
            private readonly Action _onSuccess;

            public TestExpressionTransformer(Action onSuccess)
            {
                _onSuccess = onSuccess;
            }

            public IMapperExpressionTransformer.RunPosition Position => IMapperExpressionTransformer.RunPosition.Beginning;

            public Expression<T> Transform<T>(Expression<T> source)
            {
                _onSuccess();
                return source;
            }
        }
    }
}