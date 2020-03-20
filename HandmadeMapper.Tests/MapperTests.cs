using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using HandmadeMapper.ExpressionProcessing;

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
        public void ExpressionProcessors_AreUsed()
        {
            var success = false;
            var processor = new TestExpressionTransformer(() => success = true);
            _ = new Mapper<object, object>(x => x, new[] { processor });

            // Normally, the expression should already have been processed.

            Assert.IsTrue(success, "success is false.");
        }

        [TestMethod]
        public void Map_MapsObjectFromExpression()
        {
            const int number = 1;
            var mapper = new Mapper<int,int>(x => x + 5);

            var result = mapper.Map(number);

            Assert.AreEqual(number + 5, result);
        }
        
        private class ShouldThrowExceptionTestMapper : Mapper<object, object>
        {
            public ShouldThrowExceptionTestMapper(IEnumerable<IExpressionTransformer>? expressionTransformers = null) : base(x => new object(), expressionTransformers)
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
