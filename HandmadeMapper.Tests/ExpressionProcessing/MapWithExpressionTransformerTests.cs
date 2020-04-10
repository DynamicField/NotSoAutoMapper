using HandmadeMapper.ExpressionProcessing;
using HandmadeMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace HandmadeMapper.Tests.ExpressionProcessing
{
    [TestClass]
    public class MapWithExpressionTransformerTests
    {
        private static readonly MapWithExpressionTransformer DefaultMapWithTransformer = new MapWithExpressionTransformer();

        private static readonly Mapper<Cat, CatDto> CatDtoMapper = new Mapper<Cat, CatDto>(x => new CatDto
        {
            Id = x.Id,
            Name = x.Name,
            CutenessLevel = x.CutenessLevel
        });

        [TestMethod]
        public void IEnumerable_PutsMapperExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.AsEnumerable().MapWith(CatDtoMapper).ToList()
            };
            Expression<Func<Thing, ThingDto>> expectedExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.AsEnumerable().Select(y => new CatDto
                {
                    Id = y.Id,
                    Name = y.Name,
                    CutenessLevel = y.CutenessLevel
                }).ToList()
            };

            var expression = DefaultMapWithTransformer.Transform(testExpression);

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }

        [TestMethod]
        public void IList_PutsMapperExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.MapWith(CatDtoMapper).ToList()
            };
            Expression<Func<Thing, ThingDto>> expectedExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.Select(y => new CatDto
                {
                    Id = y.Id,
                    Name = y.Name,
                    CutenessLevel = y.CutenessLevel
                }).ToList()
            };

            var expression = DefaultMapWithTransformer.Transform(testExpression);

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }
        [TestMethod]
        public void IQueryable_PutsMapperExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.AsQueryable().MapWith(CatDtoMapper).ToList()
            };
            Expression<Func<Thing, ThingDto>> expectedExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.AsQueryable().Select(y => new CatDto
                {
                    Id = y.Id,
                    Name = y.Name,
                    CutenessLevel = y.CutenessLevel
                }).ToList()
            };

            var expression = DefaultMapWithTransformer.Transform(testExpression);

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }

        [TestMethod]
        public void WithDirectRecursion_Throws()
        {
            var mapper = new Mapper<object, object>(x => x);
            Expression<Func<object, object>> expression = x => new object[] { 5 }.MapWith(mapper);

            Assert.ThrowsException<InvalidOperationException>(() => DefaultMapWithTransformer.Transform(expression,
                                                                    MappingContext.FromTypes(mapper)));
        }
    }
}