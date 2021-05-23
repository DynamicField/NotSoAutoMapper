using System;
using System.Globalization;
using System.Linq.Expressions;
using NotSoAutoMapper.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotSoAutoMapper.Tests
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
        public void Merge_ReplacesOriginalValueWithTheValueOnCommonAssignment()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Name = x.Name
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = Merge.OriginalValue<string>() + " meow!"
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto
            {
                Name = x.Name + " meow!"
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void Merge_OriginalValueWithNoFallbackOnNewAssignmentThrows()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = Merge.OriginalValue<string>()
            };

            Assert.ThrowsException<InvalidOperationException>(() => source.Merge(extension));
        }

        [TestMethod]
        public void Merge_ReplacesOriginalValueWithFallbackOnNewAssignment()
        {
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Id = x.Id
            };
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = Merge.OriginalValue("meow").ToUpper(CultureInfo.InvariantCulture)
            };
            Expression<Func<Thing, ThingDto>> expected = x => new ThingDto
            {
                Id = x.Id,
                Name = "meow".ToUpper(CultureInfo.InvariantCulture)
            };

            var merged = source.Merge(extension);

            Assert.That.ExpressionsAreEqual(expected, merged);
        }

        [TestMethod]
        public void OriginalValueNoParameter_ThrowsWhenCalled() => Assert.ThrowsException<InvalidOperationException>(Merge.OriginalValue<string>);

        [TestMethod]
        public void OriginalValueWithParameter_ThrowsWhenCalled() => Assert.ThrowsException<InvalidOperationException>(() => Merge.OriginalValue("meow"));

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

            var nameAssignment = (MemberAssignment) ((MemberInitExpression) merged.Body).Bindings[1];
            var fieldAccess = (MemberExpression) nameAssignment.Expression;
            var actualParameter = fieldAccess.Expression;

            Assert.AreSame(sourceParameter, actualParameter);
        }

        [TestMethod]
        public void Merge_WithInvalidSourceThrows()
        {
            ThingDto dto = null!;
            Expression<Func<Thing, ThingDto>> source = x => dto;
            Expression<Func<Thing, ThingDto>> extension = x => new ThingDto
            {
                Name = x.Name
            };

            Assert.ThrowsException<ArgumentException>(() => source.Merge(extension));
        }

        [TestMethod]
        public void Merge_WithInvalidExtensionThrows()
        {
            ThingDto dto = null!;
            Expression<Func<Thing, ThingDto>> source = x => new ThingDto
            {
                Name = x.Name
            };
            Expression<Func<Thing, ThingDto>> extension = x => dto;

            Assert.ThrowsException<ArgumentException>(() => source.Merge(extension));
        }
    }
}