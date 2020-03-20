using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using HandmadeMapper.ExpressionProcessing;
using HandmadeMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace HandmadeMapper.Tests.ExpressionProcessing
{
    [TestClass]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class UnwrapExpressionTransformerTests
    {
        private static readonly Expression<Func<Cat, CatDto>> CatDtoExpression = x => new CatDto
        {
            Id = x.Id,
            Name = x.Name,
            CutenessLevel = x.CutenessLevel
        };

        private static readonly Mapper<Cat, CatDto> CatDtoMapper = new Mapper<Cat, CatDto>(CatDtoExpression);

        private static readonly Expression<Func<Thing, ThingDto>> ExpectedThingDtoExpression = x => new ThingDto
        {
            Id = x.Id,
            Name = x.Name,
            FavoriteCat = new CatDto
            {
                Id = x.FavoriteCat.Id,
                Name = x.FavoriteCat.Name,
                CutenessLevel = x.FavoriteCat.CutenessLevel
            }
        };

        private static readonly UnwrapExpressionTransformer DefaultUnwrapExpressionTransformer =
            new UnwrapExpressionTransformer();

        [TestMethod]
        public void WithEfIncludeMapper_Unwraps()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = Mapper.Include(x.FavoriteCat, CatDtoMapper)
            };

            var unwrappedExpression = DefaultUnwrapExpressionTransformer.Transform(testExpression);

            Assert.That.ExpressionsAreEqual(ExpectedThingDtoExpression, unwrappedExpression);
        }

        [TestMethod]
        public void WithEfIncludeExpression_Unwraps()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = Mapper.Include(x.FavoriteCat, CatDtoExpression)
            };

            var unwrappedExpression = DefaultUnwrapExpressionTransformer.Transform(testExpression);

            Assert.That.ExpressionsAreEqual(ExpectedThingDtoExpression, unwrappedExpression);
        }

        [TestMethod]
        public void WithEfIncludeWithoutMapper_CustomMapperResolver_Unwraps()
        {
            // Arrange
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = Mapper.Include<Cat, CatDto>(x.FavoriteCat)
            };

            var mapperResolver = Substitute.For<IMapperResolver>();
            mapperResolver.ResolveMapper(Arg.Any<MethodCallExpression>()).Returns(CatDtoMapper);

            var unwrapProcessor = new UnwrapExpressionTransformer(new[] { mapperResolver });

            // Act
            var unwrappedExpression = unwrapProcessor.Transform(testExpression);

            // Assert
            Assert.That.ExpressionsAreEqual(ExpectedThingDtoExpression, unwrappedExpression);
        }

        [TestMethod]
        public void WithEfIncludeAlone_WithoutAnyMapperResolvers_Throws()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = Mapper.Include<Cat, CatDto>(x.FavoriteCat)
            };

            Assert.ThrowsException<InvalidOperationException>(() =>
                DefaultUnwrapExpressionTransformer.Transform(testExpression));
        }

        [TestMethod]
        public void WithEfIncludeMapper_WithDirectRecursion_Throws()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new DirectRecursionMapper());
        }

        private class DirectRecursionMapper : Mapper<Thing, ThingDto>
        {
            public DirectRecursionMapper() : base(new[] { DefaultUnwrapExpressionTransformer })
            {
                UseExpression(x => new ThingDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    BestFriend = Mapper.Include(x.BestFriend, this)
                });
            }
        }
    }
}