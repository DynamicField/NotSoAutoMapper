using System;
using System.Linq.Expressions;
using NotSoAutoMapper.ExpressionProcessing;
using NotSoAutoMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotSoAutoMapper.Tests.ExpressionProcessing
{
    [TestClass]
    public class MapTests
    {
        private static readonly Expression<Func<Cat, CatDto>> s_catDtoExpression = x => new CatDto
        {
            Id = x.Id,
            Name = x.Name,
            CutenessLevel = x.CutenessLevel
        };

        private static readonly Mapper<Cat, CatDto> s_catDtoMapper = new(s_catDtoExpression);

        private static readonly Expression<Func<Thing, ThingDto>> s_expectedThingDtoExpression = x => new ThingDto
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

        [TestMethod]
        public void InlinesExpression()
        {
            Expression<Func<Thing, ThingDto>> testExpression = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name,
                FavoriteCat = s_catDtoMapper.Map(x.FavoriteCat)
            };

            var unwrappedExpression = testExpression.ApplyTransformations();

            Assert.That.ExpressionsAreEqual(s_expectedThingDtoExpression, unwrappedExpression);
        }

        [TestMethod]
        public void WithNullMapper_Throws()
        {
            Expression<Func<object, int>> expression = x => ((IMapper<object, int>) null!).Map(x);

            Assert.ThrowsException<ExpressionTransformationException>(() => expression.ApplyTransformations());
        }
    }
}