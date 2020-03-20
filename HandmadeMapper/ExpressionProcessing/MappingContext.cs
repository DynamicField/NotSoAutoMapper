using System;

namespace HandmadeMapper.ExpressionProcessing
{
    /// <summary>
    /// Represents the mapping context for an expression transformation operation.
    /// </summary>
    public class MappingContext
    {
        /// <summary>
        /// Creates a <see cref="MappingContext"/>, with the specified parameters.
        /// </summary>
        /// <param name="inputType">The input type.</param>
        /// <param name="resultType">The result type.</param>
        /// <param name="mapper">The mapper.</param>
        public MappingContext(Type inputType, Type resultType, object? mapper = null)
        {
            InputType = inputType;
            ResultType = resultType;

            Mapper = mapper;
        }

        /// <summary>
        /// The input type the mapper is using.
        /// </summary>
        public Type InputType { get; }

        /// <summary>
        /// The result type the mapper is using.
        /// </summary>
        public Type ResultType { get; }

        /// <summary>
        /// The current mapper, can be null if no mapper has been specified.
        /// </summary>
        public object? Mapper { get; }

        /// <summary>
        /// Creates a mapping context from the given generic arguments.
        /// </summary>
        /// <typeparam name="TInput">The input type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <returns>A new <see cref="MappingContext"/> whose <see cref="InputType"/> is the type of <typeparamref name="TInput"/> and
        /// <see cref="ResultType"/> is the type of <typeparamref name="TResult"/></returns>
        public static MappingContext FromTypes<TInput, TResult>(object? mapper = null) =>
            new MappingContext(typeof(TInput), typeof(TResult), mapper);
    }
}