using System.Linq.Expressions;

namespace HandmadeMapper
{
    /// <summary>
    ///     Defines basic mapping expressions.
    /// </summary>
    public interface IMapperExpressionProvider
    {
        /// <summary>
        ///     The expression used to map an object to another.
        /// </summary>
        LambdaExpression Expression { get; }
    }
}