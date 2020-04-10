using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HandmadeMapper.ExpressionProcessing
{
    /// <summary>
    ///     Transforms <c>MapWith(mapper)</c> calls into the full <c>Select(x => new Thing { ... })</c> equivalent.
    /// </summary>
    public class MapWithExpressionTransformer : ExpressionVisitor, IExpressionTransformer
    {
        private static readonly MethodInfo QueryableSelectMethod
            = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(Queryable.Select));

        private static readonly MethodInfo EnumerableSelectMethod
            = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(Enumerable.Select));

        private MappingContext _context = null!;

        /// <inheritdoc cref="IExpressionTransformer.Transform{T}" />
        public Expression<T> Transform<T>(Expression<T> source, MappingContext context)
        {
            _context = context;
            return (Expression<T>) Visit(source);
        }

#pragma warning disable 1591
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods",
            Justification = "It's an expression visitor.")]
        protected override Expression VisitMethodCall(MethodCallExpression node)
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

            if (node.Method.DeclaringType != typeof(MapperEnumerableExtensions) ||
                node.Method.Name != nameof(MapperEnumerableExtensions.MapWith)) return base.VisitMethodCall(node);

            var enumerable = node.Arguments[0]; // The first argument is the enumerable (source)
            var mapperArgument = node.Arguments[1]; // The second argument is the mapper
            var enumerableType = FindEnumerableType(enumerable.Type);
            var enumerableItemType = enumerableType.GenericTypeArguments[0]; // The item type of the enumerable : IEnumerable<int> -> int

            var mapper = GetMapperFromArgument(mapperArgument);

            var selectMethod =
                FindSelectMethod(node.Method); // Gets the select method (Queryable.Select or Enumerable.Select)
            var isQueryable = selectMethod.DeclaringType == typeof(Queryable);

            var selectLambda = CreateSelectLambda(enumerableItemType, mapper);
            var selectLambdaArgument =
                isQueryable
                    ? Expression.Quote(selectLambda)
                    : (Expression) selectLambda; // If it's an IQueryable, have an Expression<T>

            var selectMethodOfType =
                selectMethod.MakeGenericMethod(enumerableItemType, selectLambda.ReturnType); // TSource, TTarget

            // Use the selectMethod with the enumerable argument, and the lambda created earlier.
            // Enumerable.Select(collection, lambda) == collection.Select(lambda)
            var selectCall = Expression.Call(null, selectMethodOfType, enumerable, selectLambdaArgument);

            return selectCall;
        }
#pragma warning restore 1591

        private static LambdaExpression CreateSelectLambda(Type enumerableItemType, IMapperExpressionProvider mapper)
        {
            var selectLambdaParameter = Expression.Parameter(enumerableItemType, "y");

            var mapperExpressionWithParameter =
                MapperExpressionUtilities.ReplaceMapperExpressionArgument(mapper, selectLambdaParameter);
            
            var selectLambda = Expression.Lambda(mapperExpressionWithParameter, selectLambdaParameter);
            return selectLambda;
        }

        private IMapperExpressionProvider GetMapperFromArgument(Expression mapperArgument)
        {
            var mapperGetter = (Func<IMapperExpressionProvider?>) Expression.Lambda(mapperArgument).Compile();
            var mapper = mapperGetter() ?? throw TransformerExceptions.InvalidMapperException;

            if (mapper == _context.Mapper) throw TransformerExceptions.RecursiveMapperException;
            return mapper;
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

        private static MethodInfo FindSelectMethod(MethodInfo mapWithMethod)
        {
            var returnType = mapWithMethod.ReturnType.GetGenericTypeDefinition();
            if (returnType == typeof(IQueryable<>)) return QueryableSelectMethod;
            if (returnType == typeof(IEnumerable<>)) return EnumerableSelectMethod;
            throw new NotSupportedException("Cannot find method for " + mapWithMethod);
        }
    }
}