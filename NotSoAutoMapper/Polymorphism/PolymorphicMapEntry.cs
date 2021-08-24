using System;
using System.Linq.Expressions;

namespace NotSoAutoMapper.Polymorphism
{
    /// <summary>
    /// A polymorphic map entry, mapping an <see cref="InputType"/> with a lazily-evaluated expression.
    /// </summary>
    /// <typeparam name="TBaseInput">The base input type.</typeparam>
    /// <typeparam name="TBaseResult">The base result type.</typeparam>
    public readonly struct PolymorphicMapEntry<TBaseInput, TBaseResult>
        where TBaseInput : notnull
        where TBaseResult : notnull
    {
        /// <summary>
        /// Creates a new <see cref="PolymorphicMapEntry{TBaseInput,TBaseResult}"/> using the given
        /// expression getter.
        /// </summary>
        /// <param name="expressionGetter">The expression getter whose result maps the subtype.</param>
        /// <typeparam name="TSubtypeInput">The subtype's input type, inheriting from the base input type.</typeparam>
        /// <typeparam name="TSubtypeResult">The subtype's result type, inheriting from the base result type.</typeparam>
        /// <returns>A <see cref="PolymorphicMapEntry{TBaseInput,TBaseResult}"/> with the given expression getter.</returns>
        public static PolymorphicMapEntry<TBaseInput, TBaseResult> Create<TSubtypeInput, TSubtypeResult>(
            Func<Expression<Func<TSubtypeInput, TSubtypeResult>>> expressionGetter)
            where TSubtypeInput : TBaseInput
            where TSubtypeResult : TBaseResult
            => new(typeof(TSubtypeInput), expressionGetter);

        private PolymorphicMapEntry(Type inputType, Func<LambdaExpression> expressionGetter)
        {
            InputType = inputType;
            ExpressionGetter = expressionGetter;
        }

        /// <summary>
        /// The input type to map. 
        /// </summary>
        public Type InputType { get; }

        /// <summary>
        /// The mapping expression getter, having a single parameter of type <see cref="InputType"/>, and a
        /// result type inheriting from <typeparamref name="TBaseResult"/>.
        /// </summary>
        public Func<LambdaExpression> ExpressionGetter { get; }
    }
}