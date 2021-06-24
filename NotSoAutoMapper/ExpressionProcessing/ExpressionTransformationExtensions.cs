using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    /// <summary>
    /// Extensions for transforming expressions.
    /// </summary>
    public static class ExpressionTransformationExtensions
    {
        private static readonly ApplyMethodTransformationsVisitor s_applyTransformationsVisitor = new();

        /// <summary>
        /// Applies all transformations on the given <paramref name="expression"/>.
        /// </summary>
        /// <remarks>
        /// Currently, this methods applies the following transformations:
        /// <list type="bullet">
        ///     <item>
        ///         Transforms all methods with the <see cref="TransformedUsingAttribute"/> 
        ///         using its transformer type.
        ///     </item>
        /// </list>
        /// </remarks>
        /// <typeparam name="T">The delegate type of the expression.</typeparam>
        /// <param name="expression">The expression to transform.</param>
        /// <returns>The transformed expression.</returns>
        public static Expression<T> ApplyTransformations<T>(this Expression<T> expression)
            => s_applyTransformationsVisitor.VisitAndConvert(expression, nameof(ApplyTransformations))!;
    }
}
