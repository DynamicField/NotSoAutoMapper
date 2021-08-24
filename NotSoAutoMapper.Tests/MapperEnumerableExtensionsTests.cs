using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReceivedExtensions;

namespace NotSoAutoMapper.Tests
{
    [TestClass]
    public class MapperEnumerableExtensionsTests
    {
        [TestMethod]
        public void MapWithEnumerable_Maps()
        {
            var mapper = CreateDefaultMapper();

            var collection = new[] {1, 2, 3};
            var expected = new[] {6, 7, 8};

            var result = collection.MapWith(mapper).ToList();

            CollectionAssert.AreEqual(expected, result);

            foreach (var item in collection)
            {
                mapper.Received(Quantity.Exactly(1)).Map(item);
            }
        }

        [TestMethod]
        public void MapWithQueryable_Maps()
        {
            var mapper = CreateDefaultMapper();

            var collection = new[] {1, 2, 3}.AsQueryable();
            var expected = new[] {6, 7, 8};

            var result = collection.MapWith(mapper).ToList();

            CollectionAssert.AreEqual(expected, result);
        }

        private static IMapper<int, int> CreateDefaultMapper()
        {
            var mapper = Substitute.For<IMapper<int, int>>();
            mapper.Expression.Returns(x => x + 5);
            mapper.Map(Arg.Any<int>()).Returns(x => mapper.Expression.Compile().Invoke(x.Arg<int>()));
            return mapper;
        }
    }
}