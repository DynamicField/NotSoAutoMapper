using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neleus.LambdaCompare;
using static System.Environment;

namespace NotSoAutoMapper.Tests.TestExtensions
{
    public static class AssertLambdaExtensions
    {
        public static void ExpressionsAreEqual(this Assert assert, Expression expected, Expression actual)
        {
            var success = Lambda.ExpressionsEqual(expected, actual);

            Assert.IsTrue(success,
                "The expressions didn't match." + NewLine + "Expected: {0}" + NewLine + "Actual: {1}", expected,
                actual);
        }
    }
}