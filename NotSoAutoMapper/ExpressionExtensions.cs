using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NotSoAutoMapper.ExpressionProcessing;

namespace NotSoAutoMapper
{
    /// <summary>
    ///     Provides extension methods for manipulating expressions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Merges two <see cref="MemberInitExpression" />s (<c>new Thing { ... }</c>) together,
        ///         taken from the bodies of <see cref="Expression{TDelegate}" />, with the following behavior:
        ///     </para>
        ///     <para>
        ///         <list type="bullet">
        ///             <item>
        ///                 Each
        ///                 <b>assignment seen on <i>both</i> the <paramref name="source" /> and <paramref name="extension" /></b>
        ///                 gets <b>replaced by the <paramref name="extension" />'s one</b>.
        ///             </item>
        ///             <item>
        ///                 Each <b>assignment seen <i>only</i> on the <paramref name="extension" /></b> (not seen in the
        ///                 <paramref name="source" />) gets <b>added</b>.
        ///             </item>
        ///             <item>
        ///                 Each <b>assignment seen <i>only</i> on the <paramref name="source" /></b> (not seen in the
        ///                 <paramref name="extension" />) is <b>leaved as it is</b>.
        ///             </item>
        ///         </list>
        ///     </para>
        ///     If both sides of an assignment is a <see cref="MemberInitExpression" />, they both get merged with the algorithm
        ///     described above.
        ///     <para>
        ///         <para>
        ///             Calls to <see cref="NotSoAutoMapper.Merge.OriginalValue{T}()" /> are also replaced.
        ///         </para>
        ///         Finally, the lambda parameters used in <paramref name="extension" /> get replaced with the ones of
        ///         <paramref name="source" />.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <para>
        ///         Source:
        ///     </para>
        ///     <code>
        /// x =&gt; new Thing
        /// {
        ///     Id = x.Id,
        ///     Name = x.Name,
        ///     Cat = new Cat 
        ///     {
        ///         Id = x.Cat.Id,
        ///         Name = x.Cat.Name
        ///     }
        /// }
        /// </code>
        ///     Extension:
        ///     <code>
        /// x =&gt; new Thing
        /// {
        ///     Name = x.Name + " is fantastic!",
        ///     Points = x.Points,
        ///     Cat = new Cat
        ///     {
        ///         Name = x.Cat.Name + ", the cat"
        ///     }
        /// }
        /// </code>
        ///     Result:
        ///     <code>
        /// x =&gt; new Thing
        /// {
        ///     Id = x.Id,
        ///     Name = x.Name + " is fantastic!",
        ///     Points = x.Points,
        ///     Cat = new Cat 
        ///     {
        ///         Id = x.Cat.Id,
        ///         Name = x.Cat.Name + ", the cat"
        ///     }
        /// }
        /// </code>
        /// </example>
        /// <typeparam name="T">The generic parameter of the <see cref="Expression{TDelegate}" />.</typeparam>
        /// <param name="source">The source expression which will be merged with <paramref name="extension" />.</param>
        /// <param name="extension">The extension expression, to merge with <paramref name="source" />.</param>
        /// <returns>The result of merging <paramref name="source" /> and <paramref name="extension" />.</returns>
        public static Expression<T> Merge<T>(this Expression<T> source, Expression<T> extension)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (extension is null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            static Exception InvalidBody(string argumentName)
            {
                return new ArgumentException("The argument does not have a MemberInitExpression body.", argumentName);
            }

            if (source.Body is not MemberInitExpression targetInit)
            {
                throw InvalidBody(nameof(source));
            }

            if (extension.Body is not MemberInitExpression extensionInit)
            {
                throw InvalidBody(nameof(extension));
            }

            var originalParameters = source.Parameters;
            var extensionParameters = extension.Parameters;

            var mergedBody = Merge(targetInit, extensionInit);
            var mergedLambda = Expression.Lambda<T>(mergedBody, originalParameters);

            // Here we use the extension's parameters as the old ones to replace them with the originals.
            // This also works reverse but it makes more sense to keep the source's parameters.
            var replacements =
                extensionParameters.Zip(originalParameters, (old, @new) => ((Expression) old, (Expression) @new));
            var replacer = new ReplacerVisitor(replacements);

            return (Expression<T>) replacer.Replace(mergedLambda);
        }

        private static MemberInitExpression Merge(MemberInitExpression source, MemberInitExpression extension)
        {
            var originalValueVisitor = new OriginalValueVisitor();

            var sourceAssignments = source.Bindings.OfType<MemberAssignment>().ToList();
            var extensionAssignments = extension.Bindings.OfType<MemberAssignment>().ToList();

            var commonAssignments =
                (from targetAssignment in sourceAssignments
                    join extensionAssignment in extensionAssignments on targetAssignment.Member equals
                        extensionAssignment.Member
                    select (targetAssignment, extensionAssignment)).ToList();

            // There we also replace OriginalValue<T>(), but it only takes the fallback value, or throws.
            var newAssignments = extensionAssignments.Except(commonAssignments.Select(c => c.extensionAssignment))
                .Select(x => x.Update(originalValueVisitor.ReplaceOriginalValue(x.Expression, null)));

            var mergedBindings = new List<MemberBinding>(sourceAssignments.Concat(newAssignments));

            foreach (var (sourceAssignment, extensionAssignment) in commonAssignments)
            {
                mergedBindings.Remove(sourceAssignment);

                MemberAssignment assignment;

                // In the case we have two MemberInitExpressions, merge them. 
                if (sourceAssignment.Expression is MemberInitExpression sourceAssignmentMemberInit &&
                    extensionAssignment.Expression is MemberInitExpression extensionAssignmentMemberInit)
                {
                    var mergedExpression = Merge(sourceAssignmentMemberInit, extensionAssignmentMemberInit);

                    assignment = extensionAssignment.Update(mergedExpression);
                }
                else
                {
                    assignment = extensionAssignment;
                }

                // Look for some OriginalValue<T>() and replace them.
                var processedExpression =
                    originalValueVisitor.ReplaceOriginalValue(assignment.Expression, sourceAssignment);
                assignment = assignment.Update(processedExpression);

                mergedBindings.Add(assignment);
            }

            return source.Update(source.NewExpression, mergedBindings);
        }

        private class OriginalValueVisitor : ExpressionVisitor
        {
            private MemberAssignment? _sourceAssignment;

            public Expression ReplaceOriginalValue(Expression expression, MemberAssignment? sourceAssignment)
            {
                _sourceAssignment = sourceAssignment;
                return Visit(expression);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                // Make sure we have the base generic method: OriginalValue<T>
                var method = node.Method.IsGenericMethod ? node.Method.GetGenericMethodDefinition() : node.Method;

                if (method != MergeMethods.OriginalValueWithFallbackMethod &&
                    method != MergeMethods.OriginalValueNoParamsMethod)
                {
                    return base.VisitMethodCall(node);
                }

                var originalValue = _sourceAssignment?.Expression;

                originalValue = originalValue switch
                {
                    // Use fallback
                    null when method == MergeMethods.OriginalValueWithFallbackMethod => node.Arguments[0],
                    // No fallback? You're screwed!
                    null when method == MergeMethods.OriginalValueNoParamsMethod => throw NotSoAutoMapper.Merge
                        .OriginalValueException(),
                    _ => originalValue
                };

                return Visit(originalValue);
            }
        }
    }

    /// <summary>
    ///     Contains helper methods for the <see cref="ExpressionExtensions.Merge{T}" /> method.
    /// </summary>
    public static class Merge
    {
        /// <summary>
        ///     <para>
        ///         Gets the original value (of type <typeparamref name="T" />) of the assignment.
        ///     </para>
        ///     <para>
        ///         If the original value has not been found and no fallback value has been specified,
        ///         this method will throw an <see cref="InvalidOperationException" />.
        ///     </para>
        /// </summary>
        /// <typeparam name="T">The type of the original value.</typeparam>
        /// <exception cref="InvalidOperationException">
        ///     When the original value has not been found and no fallback value has been
        ///     specified.
        /// </exception>
        /// <returns>The original value (from the right-hand side of the original assignment).</returns>
        public static T OriginalValue<T>() => throw OriginalValueException();


        /// <inheritdoc cref="OriginalValue{T}()" />
        /// <param name="fallback">The fallback value to use.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Used in expressions.")]
        public static T OriginalValue<T>(T fallback) => throw OriginalValueException();

        internal static InvalidOperationException OriginalValueException() => new(
                $"{nameof(OriginalValue)} has failed, this is due to one of these reasons:{Environment.NewLine}" +
                $"- This method has been called outside of the {nameof(ExpressionExtensions.Merge)} method.{Environment.NewLine}" +
                "- The original value has not been found in the original expression, and no fallback value has been provided.");
    }

    internal static class MergeMethods
    {
        public static readonly MethodInfo OriginalValueNoParamsMethod = OriginalValueOfParamsLength(0);

        public static readonly MethodInfo OriginalValueWithFallbackMethod = OriginalValueOfParamsLength(1);

        private static MethodInfo OriginalValueOfParamsLength(int length) => typeof(Merge).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(Merge.OriginalValue) && x.GetParameters().Length == length);
    }
}