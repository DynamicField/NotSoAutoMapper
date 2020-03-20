using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HandmadeMapper.ExpressionProcessing;

namespace HandmadeMapper
{
    /// <summary>
    /// Provides extension methods for manipulating expressions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// <para>
        /// Merges two <see cref="MemberInitExpression"/>s together, taken from the bodies of <see cref="Expression{TDelegate}"/>, with the following behavior:
        /// </para>
        /// <para>
        /// <list type="bullet">
        /// <item>
        /// Each <b>assignment seen on <i>both</i> the <paramref name="source"/> and <paramref name="extension"/></b> gets <b>replaced by the <paramref name="extension"/>'s one</b>.
        /// </item>
        /// <item>
        /// Each <b>assignment seen <i>only</i> on the <paramref name="extension"/></b> (not seen in the <paramref name="source"/>) gets <b>added</b>.
        /// </item>
        /// <item>
        /// Each <b>assignment seen <i>only</i> on the <paramref name="source"/></b> (not seen in the <paramref name="extension"/>) is <b>leaved as it is</b>.
        /// </item>
        /// </list>
        /// </para>
        /// If both sides of an assignment is a <see cref="MemberInitExpression"/>, they both get merged with the algorithm described above.
        /// <para>
        /// Finally, the lambda parameters used in <paramref name="extension"/> get replaced with the ones of <paramref name="source"/>.
        /// </para>
        /// </summary>
        /// <example>
        /// <para>
        /// Source:
        /// </para>
        /// <code>
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
        /// Extension:
        /// <code>
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
        /// Result:
        /// <code>
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
        /// <typeparam name="T">The generic parameter of the <see cref="Expression{TDelegate}"/>.</typeparam>
        /// <param name="source">The source expression which will be merged with <paramref name="extension"/>.</param>
        /// <param name="extension">The extension expression, to merge with <paramref name="source"/>.</param>
        /// <returns>The result of merging <paramref name="source"/> and <paramref name="extension"/>.</returns>
        public static Expression<T> Merge<T>(this Expression<T> source, Expression<T> extension)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            if (extension is null)
                throw new ArgumentNullException(nameof(extension));

            static Exception InvalidBody(string argumentName)
            {
                return new ArgumentException("The argument does not have a MemberInitExpression body.", argumentName);
            }

            if (!(source.Body is MemberInitExpression targetInit))
                throw InvalidBody(nameof(source));

            if (!(extension.Body is MemberInitExpression extensionInit))
                throw InvalidBody(nameof(extension));

            var originalParameters = source.Parameters;
            var extensionParameters = extension.Parameters;

            var mergedBody = Merge(targetInit, extensionInit);
            var mergedLambda = Expression.Lambda<T>(mergedBody, originalParameters);

            // Here we use the extension's parameters as the old ones to replace them with the originals.
            // This also works reverse but it makes more sense to keep the source's parameters.
            var replacements = extensionParameters.Zip(originalParameters, (old, @new) => ((Expression)old, (Expression)@new));
            var replacer = new ReplacerVisitor(replacements);

            return (Expression<T>)replacer.Replace(mergedLambda);
        }

        private static MemberInitExpression Merge(MemberInitExpression source, MemberInitExpression extension)
        {
            var targetBindings = source.Bindings.OfType<MemberAssignment>().ToList();
            var extensionBindings = extension.Bindings.OfType<MemberAssignment>().ToList();

            var commonBindings =
                (from targetBinding in targetBindings
                 join extensionBinding in extensionBindings on targetBinding.Member equals extensionBinding.Member
                 select (targetBinding, extensionBinding)).ToList();

            var newBindings = extensionBindings.Except(commonBindings.Select(c => c.extensionBinding));

            var mergedBindings = new List<MemberBinding>(source.Bindings.Concat(newBindings));

            foreach (var (targetBinding, extensionBinding) in commonBindings)
            {
                mergedBindings.Remove(targetBinding);

                // In the case we have two MemberInitExpressions, merge them. 
                if (targetBinding.Expression is MemberInitExpression targetBindingMemberInit &&
                    extensionBinding.Expression is MemberInitExpression extensionBindingMemberInit)
                {
                    var merged = Merge(targetBindingMemberInit, extensionBindingMemberInit);
                    mergedBindings.Add(Expression.Bind(targetBinding.Member, merged));
                }
                else
                {
                    mergedBindings.Add(extensionBinding);
                }
            }

            return source.Update(source.NewExpression, mergedBindings);
        }
    }
}