using System;
using System.Linq.Expressions;
using HandmadeMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HandmadeMapper.Tests
{
    [TestClass]
    public class ExpressionExtensionsTests
    {
        [TestMethod]
        public void Merge_AddsNewAssignments()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = x.Name
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_ReplacesCommonAssignmentsWithThoseInTheExtension()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = x.Name + " is fantastic!"
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto
            {
                Id = x.Id,
                Name = x.Name + " is fantastic!"
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_KeepsSourceConstructor()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto("meow")
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto("meow")
            {
                Id = x.Id
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_MergesInnerMemberInitExpressions()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                FavoriteCat = new CatDto
                {
                    Id = x.FavoriteCat.Id,
                    Name = x.FavoriteCat.Name
                }
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                FavoriteCat = new CatDto
                {
                    Name = x.FavoriteCat.Name + " meow!",
                    CutenessLevel = 99
                }
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto
            {
                FavoriteCat = new CatDto
                {
                    Id = x.FavoriteCat.Id,
                    Name = x.FavoriteCat.Name + " meow!",
                    CutenessLevel = 99
                }
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_ReplacesLambdaParameterOfTheTargetWithTheSourceOne()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = x.Name
            };
            var sourceParameter = source.Parameters[0];

            var merged = source.Merge(extension);

            var nameAssignment = (MemberAssignment)((MemberInitExpression)merged.Body).Bindings[1];
            var fieldAccess = (MemberExpression) nameAssignment.Expression;
            var actualParameter = fieldAccess.Expression;

            Assert.AreSame(sourceParameter, actualParameter);
        }
    }
}