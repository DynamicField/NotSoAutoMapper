using System.Linq.Expressions;

namespace HandmadeMapper.ExpressionProcessing
{
    internal static class MapperExpressionUtilities
    {
        internal static Expression ReplaceMapperExpressionArgument(IMapperExpressionProvider mapper,
            Expression sourceArgument)
        {
            return ReplaceMapperExpressionArgument(mapper.Expression, sourceArgument);
        }

        internal static Expression ReplaceMapperExpressionArgument(LambdaExpression mapperExpression,
            Expression sourceArgument)
        {
            // We get a lambda expression from the mapper:
            //  y => new Something { Cat = y.Cat }
            // Then grab the initial source: y
            var mapperInitialSource = mapperExpression.Parameters[0];

            // Now we replace y with x.Thing, in the body:
            //  new Something { Cat = y.Cat } -> new Something { Cat = x.Thing.Cat }
            var replacer = new ReplacerVisitor(mapperInitialSource, sourceArgument);
            var finalExpression = replacer.Replace(mapperExpression.Body);

            return finalExpression;
        }
    }
}