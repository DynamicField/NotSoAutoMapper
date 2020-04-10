using System;
using System.Linq.Expressions;
using HandmadeMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace HandmadeMapper.Tests
{
    [TestClass]
    public class MapperExtensionsTests
    {
        [TestMethod]
        public void Merge_MergesExpressionWithExtension()
        {
            Expression<Func<Thing, ThingDto>> expression = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = x.Name
            };
            var expectedExpression = expression.Merge(extension);
            var mapper = CreateMapperSubstitute(expression);

            var result = mapper.Merge(extension);

            _ = mapper.Received(Quantity.AtLeastOne()).Expression; // Only the Expression should get used.
            Assert.That.ExpressionsAreEqual(expectedExpression, result.Expression);
        }

        [TestMethod]
        public void MergeOriginal_MergesOriginalExpressionWithExtension()
        {
            Expression<Func<Thing, ThingDto>> expression = x => new ThingDto();
            Expression<Func<Thing, ThingDto>> originalExpression = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = x.Name
            };
            var expectedExpression = originalExpression.Merge(extension);
            var mapper = CreateMapperSubstitute(expression, originalExpression);

            var result = mapper.MergeOriginal(extension);

            _ = mapper.Received(Quantity.AtLeastOne()).OriginalExpression; // Only the OriginalExpression should get used.
            Assert.That.ExpressionsAreEqual(expectedExpression, result.OriginalExpression);
        }

        private static IMapper<TInput, TResult> CreateMapperSubstitute<TInput, TResult>(
            Expression<Func<TInput, TResult>> expression, Expression<Func<TInput, TResult>>? originalExpression = null)
        {
            var mapper = Substitute.For<IMapper<TInput, TResult>>();
            mapper.OriginalExpression.Returns(originalExpression ?? expression);
            mapper.Expression.Returns(expression);
            mapper.WithExpression(Arg.Any<Expression<Func<TInput, TResult>>>())
                .Returns(call => CreateMapperSubstitute(call.Arg<Expression<Func<TInput, TResult>>>()));
            return mapper;
        }
    }
}