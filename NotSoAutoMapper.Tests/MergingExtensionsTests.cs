using System;
using System.Globalization;
using System.Linq.Expressions;
using NotSoAutoMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotSoAutoMapper.Tests
{
    [TestClass]
    public class MergingExtensionsTests
    {
        [TestMethod]
        public void Merge_AddsNewAssignments()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Name = x.Name };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto { Id = x.Id, Name = x.Name };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_ReplacesCommonAssignmentsWithThoseInTheExtension()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id, Name = x.Name };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Name = x.Name + " is fantastic!" };
            Expression<Func<Thing, ThingDto>> expected = x =>
                new ThingDto { Id = x.Id, Name = x.Name + " is fantastic!" };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_IgnoresSourceConstructor()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto("meow") { Id = x.Id };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Id = x.Id };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto { Id = x.Id };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_MergesInnerMemberInitExpressions()
        {
            Expression<Func<Thing, ThingDto>> source = x =>
                new ThingDto {
                    FavoriteCat = new CatDto { Id = x.FavoriteCatNotNull.Id, Name = x.FavoriteCatNotNull.Name }
                };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto {
                FavoriteCat = new CatDto { Name = x.FavoriteCatNotNull.Name + " meow!", CutenessLevel = 99 }
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto {
                FavoriteCat = new CatDto {
                    Id = x.FavoriteCatNotNull.Id, Name = x.FavoriteCatNotNull.Name + " meow!", CutenessLevel = 99
                }
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_ReplacesOriginalValue()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Name = x.Name };
            Expression<Func<Thing, ThingDto>> extension = x =>
                new ThingDto { Name = Merge.OriginalValue<string>() + " meow!" };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto { Name = x.Name + " meow!" };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_OriginalValueWithNoFallbackOnNewAssignmentThrows()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Name = Merge.OriginalValue<string>() };

            Assert.ThrowsException<InvalidOperationException>(() => source.Merge(extension));
        }

        [TestMethod]
        public void Merge_ReplacesOriginalValueWithFallback()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id };
            Expression<Func<Thing, ThingDto>> extension = x =>
                new ThingDto { Name = Merge.OriginalValue("meow").ToUpper(CultureInfo.InvariantCulture) };
            Expression<Func<Thing, ThingDto>> expected = x =>
                new ThingDto { Id = x.Id, Name = "meow".ToUpper(CultureInfo.InvariantCulture) };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void OriginalValueNoParameter_ThrowsWhenCalled() =>
            Assert.ThrowsException<InvalidOperationException>(Merge.OriginalValue<string>);

        [TestMethod]
        public void OriginalValueWithParameter_ThrowsWhenCalled() =>
            Assert.ThrowsException<InvalidOperationException>(() => Merge.OriginalValue("meow"));

        [TestMethod]
        public void Merge_ReplacesLambdaParameterOfTheSourceWithTheTargetOne()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Name = x.Name };
            var sourceParameter = source.Parameters[0];

            var merged = source.Merge(extension);

            var nameAssignment = (MemberAssignment) ((MemberInitExpression) merged.Body).Bindings[1];
            var fieldAccess = (MemberExpression) nameAssignment.Expression;
            var actualParameter = fieldAccess.Expression;

            Assert.AreNotSame(sourceParameter, actualParameter);
        }

        [TestMethod]
        public void Merge_WithInvalidSourceThrows()
        {
            ThingDto dto = null!;
            Expression<Func<Thing, ThingDto>> source = x => dto;
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Name = x.Name };

            Assert.ThrowsException<ArgumentException>(() => source.Merge(extension));
        }

        [TestMethod]
        public void Merge_WithInvalidExtensionThrows()
        {
            ThingDto dto = null!;
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Name = x.Name };
            Expression<Func<Thing, ThingDto>> extension = x => dto;

            Assert.ThrowsException<ArgumentException>(() => source.Merge(extension));
        }

        // Derived tests

        [TestMethod]
        public void Merge_Derived_AddsDerivedProperties()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id };
            Expression<Func<BetterThing, BetterThingDto>> extension = x =>
                new BetterThingDto { FavoriteSentence = x.FavoriteSentence };
            Expression<Func<BetterThing, BetterThingDto>> expected = x =>
                new BetterThingDto { Id = x.Id, FavoriteSentence = x.FavoriteSentence };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_Derived_ReplacesOriginalValue()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto { Id = x.Id, Name = x.Name };
            Expression<Func<BetterThing, BetterThingDto>> extension = x =>
                new BetterThingDto {
                    Name = Merge.OriginalValue<string>() + " meow!", FavoriteSentence = x.FavoriteSentence
                };
            Expression<Func<BetterThing, BetterThingDto>> expected = x =>
                new BetterThingDto { Id = x.Id, Name = x.Name + " meow!", FavoriteSentence = x.FavoriteSentence };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        // Mapper merging

        [TestMethod]
        public void Merge_MergesExpressionWithExtension()
        {
            Expression<Func<Thing, ThingDto>> expression = x => new ThingDto { Id = x.Id };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto { Name = x.Name };
            var expectedExpression = expression.Merge(extension);
            var mapper = new Mapper<Thing, ThingDto>(expression);

            var result = mapper.Merge(extension);
            
            Assert.That.ExpressionsAreEqual(expectedExpression, result.Expression);
        }
    }
}