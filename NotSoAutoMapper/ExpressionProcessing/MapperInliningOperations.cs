using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal static class MapperInliningOperations
    {
        private static readonly MethodInfo s_queryableSelectMethod
            = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(Queryable.Select));

        private static readonly MethodInfo s_enumerableSelectMethod
            = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(Enumerable.Select));

        public static Expression InlineCollection(
            Expression sourceExpression, Expression mapperExpression)
        {
            // collection.MapWith(mapper) -> collection.Select(mapper.Expression)
            // -----
            // However, Enumerable.Select does not support expressions and EF does not like that either (ICollection<T>), so:
            // -----
            //  collection.Select(y => new Thing { Name = x.Name })
            // -----
            // Then replace x with y:
            // -----
            //  collection.Select(y => new Thing { Name = y.Name })

            var mapper = mapperExpression.CompileAndGet<IMapper>() ?? throw TransformerExceptions.NullMapperException;

            // The item type of the source : IEnumerable<int> -> int
            var sourceItemType = FindEnumerableType(sourceExpression.Type).GenericTypeArguments[0];
            var selectMethod = FindPreferredSelectMethod(sourceExpression); // Queryable.Select or Enumerable.Select

            // Take the mapper's expression, and rename its lambda parameter to avoid naming collisions
            var selectLambda = CreateSelectLambda(sourceItemType, mapper);
            // Make sure we use the correct expression type for the Select method
            var selectLambdaArgument = selectMethod.DeclaringType == typeof(Queryable)
                    ? Expression.Quote(selectLambda) // As Expression<Func<...>>
                    : (Expression) selectLambda;  // As Func<...>

            // Create a Select method with the new generic arguments:
            // Select<TEnumerableItem, TMapperResult>()
            var selectMethodOfType =
                selectMethod.MakeGenericMethod(sourceItemType, selectLambda.ReturnType); // TSource, TTarget

            // Use the selectMethod with the enumerable argument, and the lambda created earlier.
            // Enumerable.Select(collection, lambda) == collection.Select(lambda)
            return Expression.Call(null, selectMethodOfType, sourceExpression, selectLambdaArgument);

            static LambdaExpression CreateSelectLambda(Type enumerableItemType, IMapper mapper)
            {
                var selectLambdaParameter = Expression.Parameter(enumerableItemType, "map_" + Guid.NewGuid().ToString("N"));

                var mapperExpressionWithParameter = ReplaceMapperExpressionArgument(mapper, selectLambdaParameter);

                var selectLambda = Expression.Lambda(mapperExpressionWithParameter, selectLambdaParameter);
                return selectLambda;
            }
        }

        public static Expression InlineObject(Expression sourceExpression, Expression mapperExpression)
        {
            var mapper = mapperExpression.CompileAndGet<IMapper>();
            if (mapper is null)
            {
                throw TransformerExceptions.NullMapperException;
            }

            return ReplaceMapperExpressionArgument(mapper, sourceExpression);
        }

        private static Type FindEnumerableType(Type type)
        {
            static bool IsEnumerable(Type type, object? whatever = null) // The whatever is for TypeFilter
            {
                return type.IsGenericType &&
                       type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            }

            return IsEnumerable(type) ? type : type.FindInterfaces(IsEnumerable, null)[0];
        }

        private static MethodInfo FindPreferredSelectMethod(Expression collectionExpression)
        {
            var collectionType = collectionExpression.Type;
            var interfaces = collectionType.GetInterfaces();

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IQueryable<>) ||
                interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>)))
            {
                return s_queryableSelectMethod;
            }
            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                return s_enumerableSelectMethod;
            }

            throw new NotSupportedException($"Cannot find method for collection type {collectionType}.");
        }

        private static Expression ReplaceMapperExpressionArgument(IMapper mapper, Expression sourceArgument) 
            => ReplaceMapperExpressionArgument(mapper.Expression, sourceArgument);

        private static Expression ReplaceMapperExpressionArgument(LambdaExpression mapperExpression,
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
