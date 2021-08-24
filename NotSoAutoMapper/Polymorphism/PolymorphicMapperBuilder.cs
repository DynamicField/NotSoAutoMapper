using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using NotSoAutoMapper.ExpressionProcessing;

namespace NotSoAutoMapper.Polymorphism
{
    /// <summary>
    /// Builds a <see cref="PolymorphicMapper{TBaseInput,TResult}"/> — mapping an object inheritance tree —
    /// with a fluent API used to provide mappings for subtypes. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// The builder can be used to map the base type and its subtypes, using the
    /// <see cref="MapType{TSubtypeInput,TSubtypeResult}(System.Func{System.Linq.Expressions.Expression{System.Func{TSubtypeInput,TSubtypeResult}}},bool)">
    /// <c>MapType</c></see> methods.
    /// <br/> When mapping a type that is already mapped, the previous mapping will be replaced. 
    /// </para>
    /// <para>
    /// More details about the final generated expression can be found in <see cref="PolymorphicMapper{TBaseInput,TResult}"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// This example shows how to map an inheritance tree with three classes: Base, Derived1 and Derived2.
    /// Base's <c>PolymorphicMapper</c> field, containing the mapper, can be used to, well, map an object!
    /// <code>
    /// public abstract record Base(int SharedProperty);
    /// public record Derived1(int SharedProperty, int InterestingProperty) : Base(SharedProperty);
    /// public record Derived2(int SharedProperty, int SpecialProperty) : Base(SharedProperty);
    /// 
    /// public class BaseDto
    /// {
    ///     protected BaseDto() {}
    ///     public int SharedProperty { get; init; }
    /// 
    ///     protected static readonly Mapper&lt;Base, BaseDto&gt; BaseMapper = new(x =&gt; new BaseDto
    ///     {
    ///         SharedProperty = x.SharedProperty
    ///     });
    /// 
    ///     public static readonly PolymorphicMapper&lt;Base, BaseDto&gt; PolymorphicMapper =
    ///         new PolymorphicMapperBuilder&lt;Base, BaseDto&gt;()
    ///             .MapType(() =&gt; Derived1Dto.Mapper)
    ///             .MapType(() =&gt; Derived2Dto.Mapper)
    ///             .Build();
    /// }
    /// 
    /// public class Derived1Dto : BaseDto
    /// {
    ///     public int InterestingProperty { get; init; }
    /// 
    ///     public static readonly Mapper&lt;Derived1, Derived1Dto&gt; Mapper = BaseMapper.Merge((Derived1 x) =&gt; new Derived1Dto
    ///     {
    ///         InterestingProperty = x.InterestingProperty
    ///     });
    /// }
    /// 
    /// public class Derived2Dto : BaseDto
    /// {
    ///     public int SpecialProperty { get; init; }
    /// 
    ///     public static readonly Mapper&lt;Derived2, Derived2Dto&gt; Mapper = BaseMapper.Merge((Derived2 x) =&gt; new Derived2Dto
    ///     {
    ///         SpecialProperty = x.SpecialProperty
    ///     });
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="TBaseInput">The base input type.</typeparam>
    /// <typeparam name="TBaseResult">The base result type.</typeparam>
    /// <seealso cref="PolymorphicMapper{TBaseInput,TResult}"/>
    /// <seealso cref="PolymorphicMapping{TBaseInput,TBaseResult}"/>
    public class PolymorphicMapperBuilder<TBaseInput, TBaseResult>
        where TBaseInput : notnull
        where TBaseResult : notnull
    {
        private readonly List<PolymorphicMapEntry<TBaseInput, TBaseResult>> _entries = new();

        private static PolymorphicMapEntry<TBaseInput, TBaseResult> MakeEntry<TSubtypeInput, TSubtypeResult>(
            Func<Expression<Func<TSubtypeInput, TSubtypeResult>>> expressionGetter, bool applyTransformations = true)
            where TSubtypeInput : TBaseInput
            where TSubtypeResult : TBaseResult
        {
            var transformedExpressionGetter = expressionGetter;
            if (applyTransformations)
            {
                transformedExpressionGetter = () => expressionGetter().ApplyTransformations();
            }

            return PolymorphicMapEntry<TBaseInput, TBaseResult>.Create(transformedExpressionGetter);
        }
        
        /// <summary>
        /// Maps a type using the given lazily-evaluated expression.
        /// </summary>
        /// <param name="expressionGetter">The expression getter whose result maps the type.</param>
        /// <param name="applyTransformations">Whether or not transformations should be applied.</param>
        /// <typeparam name="TSubtypeInput">The input type, optionally inheriting from the base input type.</typeparam>
        /// <typeparam name="TSubtypeResult">The result type, optionally inheriting from the base result type.</typeparam>
        /// <returns>The same builder.</returns>
        public PolymorphicMapperBuilder<TBaseInput, TBaseResult> MapType<TSubtypeInput, TSubtypeResult>(
            Func<Expression<Func<TSubtypeInput, TSubtypeResult>>> expressionGetter, bool applyTransformations = true)
            where TSubtypeInput : TBaseInput
            where TSubtypeResult : TBaseResult
        {
            var existingBaseTypeMappingIndex = _entries.FindIndex(x => x.InputType == typeof(TBaseInput));
            if (existingBaseTypeMappingIndex != -1)
            {
                _entries.RemoveAt(existingBaseTypeMappingIndex);
            }
            
            _entries.Add(MakeEntry(expressionGetter, applyTransformations));
            return this;
        }

        /// <summary>
        /// Maps a type using the given expression.
        /// </summary>
        /// <param name="expression">The mapping expression.</param>
        /// <param name="applyTransformations">Whether or not transformations should be applied.</param>
        /// <typeparam name="TSubtypeInput">The input type, optionally inheriting from the base input type.</typeparam>
        /// <typeparam name="TSubtypeResult">The result type, optionally inheriting from the base result type.</typeparam>
        /// <returns>The same builder.</returns>
        public PolymorphicMapperBuilder<TBaseInput, TBaseResult> MapType<TSubtypeInput, TSubtypeResult>(
            Expression<Func<TSubtypeInput, TSubtypeResult>> expression, bool applyTransformations = true)
            where TSubtypeInput : TBaseInput
            where TSubtypeResult : TBaseResult => MapType(() => expression, applyTransformations);

        /// <summary>
        /// Maps a type using the given mapper.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <typeparam name="TSubtypeInput">The input type, optionally inheriting from the base input type.</typeparam>
        /// <typeparam name="TSubtypeResult">The result type, optionally inheriting from the base result type.</typeparam>
        /// <returns>The same builder.</returns>
        public PolymorphicMapperBuilder<TBaseInput, TBaseResult> MapType<TSubtypeInput, TSubtypeResult>(
            IMapper<TSubtypeInput, TSubtypeResult> mapper)
            where TSubtypeInput : TBaseInput
            where TSubtypeResult : TBaseResult
            => MapType(() => mapper.Expression, false);

        /// <summary>
        /// Maps a type using the given lazily-evaluated mapper.
        /// </summary>
        /// <param name="mapperGetter">The mapper getter whose result maps the type.</param>
        /// <typeparam name="TSubtypeInput">The input type, optionally inheriting from the base input type.</typeparam>
        /// <typeparam name="TSubtypeResult">The result type, optionally inheriting from the base result type.</typeparam>
        /// <returns>The same builder.</returns>
        public PolymorphicMapperBuilder<TBaseInput, TBaseResult> MapType<TSubtypeInput, TSubtypeResult>(
            Func<IMapper<TSubtypeInput, TSubtypeResult>> mapperGetter)
            where TSubtypeInput : TBaseInput
            where TSubtypeResult : TBaseResult => MapType(() => mapperGetter().Expression, false);

        /// <summary>
        /// Builds the <see cref="PolymorphicMapper{TBaseInput,TResult}"/>.
        /// </summary>
        /// <returns>The built <see cref="PolymorphicMapper{TBaseInput,TResult}"/>.</returns>
        public PolymorphicMapper<TBaseInput, TBaseResult> Build() 
            => new(new PolymorphicMapping<TBaseInput, TBaseResult>(_entries));
    }
}