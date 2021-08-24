using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotSoAutoMapper.Tests
{
    [TestClass]
    public class MapperTests
    {
        [TestMethod]
        public void Map_MapsObjectFromExpression()
        {
            const int Number = 1;
            var mapper = new Mapper<int, int>(x => x + 5);

            var result = mapper.Map(Number);

            Assert.AreEqual(Number + 5, result);
        }

        [TestMethod]
        public void WithExpression_ChangesExpression()
        {
            var mapper = new Mapper<int, int>(x => x + 5);
            Expression<Func<int, int>> newExpression = x => x + 10;

            var newMapper = mapper.WithExpression(newExpression);

            Assert.AreSame(newExpression, newMapper.OriginalExpression);
        }

        [TestMethod]
        public void Map_MapsNullToNull()
        {
            var mapper = new Mapper<object, object>(x => new object());

            var result = mapper.Map(null);
            
            Assert.IsNull(result);
        }
    }
}