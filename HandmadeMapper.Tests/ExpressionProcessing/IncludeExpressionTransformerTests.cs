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
    public class IncludeExpressionTransformerTests
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

        private static readonly IncludeExpressionTransformer DefaultIncludeExpressionTransformer =
            new IncludeExpressionTransformer();

        [TestMethod]
        public void WithEfIncludeMapper_Unwraps()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = Mapper.Include(x.FavoriteCat, CatDtoMapper)
            };

            var unwrappedExpression = DefaultIncludeExpressionTransformer.Transform(testExpression);

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

            var unwrappedExpression = DefaultIncludeExpressionTransformer.Transform(testExpression);

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

            var includeTransformer = new IncludeExpressionTransformer(new[] {mapperResolver});

            // Act
            var unwrappedExpression = includeTransformer.Transform(testExpression);

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
                DefaultIncludeExpressionTransformer.Transform(testExpression));
        }

        [TestMethod]
        public void WithEfIncludeMapper_WithDirectRecursion_Throws()
        {
            Assert.ThrowsException<InvalidOperationException>(() => new DirectRecursionMapper());
        }

        [TestMethod]
        public void WithNullMapper_Throws()
        {
            Expression<Func<object, int>> expression = x =>
                Mapper.Include(x, (IMapper<object, int>)null!);

            Assert.ThrowsException<InvalidOperationException>(() => 
                DefaultIncludeExpressionTransformer.Transform(expression));
        }

        private class DirectRecursionMapper : Mapper<Thing, ThingDto>
        {
            public DirectRecursionMapper() : base(new[] {DefaultIncludeExpressionTransformer})
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