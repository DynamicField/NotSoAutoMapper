using NotSoAutoMapper.ExpressionProcessing;
using NotSoAutoMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace NotSoAutoMapper.Tests.ExpressionProcessing
{
    [TestClass]
    public class MapWithCollectionOrObjectTests
    {
        private static readonly Mapper<Cat, CatDto> s_catDtoMapper = new(x => new CatDto
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
                Cats = x.Cats.AsEnumerable().MapWith(s_catDtoMapper).ToList()
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

            var expression = testExpression.ApplyTransformations();

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }

        [TestMethod]
        public void IList_PutsMapperExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.MapWith(s_catDtoMapper).ToList()
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

            var expression = testExpression.ApplyTransformations();

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }
        [TestMethod]
        public void IQueryable_PutsMapperExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                Cats = x.Cats.AsQueryable().MapWith(s_catDtoMapper).ToList()
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

            var expression = testExpression.ApplyTransformations();

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }

        [TestMethod]
        public void Object_PutsMapperExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = x.FavoriteCat.MapWith(s_catDtoMapper)
            };
            Expression<Func<Thing, ThingDto>> expectedExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = x.FavoriteCat == null ? null : new CatDto
                {
                    Id = x.FavoriteCat.Id,
                    Name = x.FavoriteCat.Name,
                    CutenessLevel = x.FavoriteCat.CutenessLevel
                }
            };

            var expression = testExpression.ApplyTransformations();

            Assert.That.ExpressionsAreEqual(expectedExpression, expression);
        }

        [TestMethod]
        public void RootQueryable_HasExpression()
        {
            var mapperExpression = s_catDtoMapper.Expression;
            var baseQueryable = Enumerable.Empty<Cat>().AsQueryable();
            
            var mappedQueryable = baseQueryable.MapWith(s_catDtoMapper);

            // EnumerableQuery<Cat>.Select([expression])
            var selectQueryableExpression = 
                ((UnaryExpression) ((MethodCallExpression) mappedQueryable.Expression).Arguments[1]).Operand;
            Assert.That.ExpressionsAreEqual(mapperExpression, selectQueryableExpression);
        }
    }
}