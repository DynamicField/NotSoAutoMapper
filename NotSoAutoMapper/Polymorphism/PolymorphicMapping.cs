using System;
using System.Collections.Generic;
using System.Linq;

namespace NotSoAutoMapper.Polymorphism
{
    /// <summary>
    /// Describes how to map an inheritance tree using a mapping expression for each subtype.
    /// </summary>
    /// <typeparam name="TBaseInput">The base input type.</typeparam>
    /// <typeparam name="TBaseResult">The base result type.</typeparam>
    public class PolymorphicMapping<TBaseInput, TBaseResult>
        where TBaseInput : notnull
        where TBaseResult : notnull
    {
        /// <summary>
        /// Creates a <see cref="PolymorphicMapping{TBaseInput,TBaseResult}"/> using the given list of
        /// map entries.
        /// </summary>
        /// <param name="entries">The entries to use.</param>
        /// <exception cref="ArgumentException">When <c>entries</c> is empty.</exception>
        /// <exception cref="ArgumentException">When <c>entries</c> has duplicate items having the same input type.</exception>
        public PolymorphicMapping(IEnumerable<PolymorphicMapEntry<TBaseInput, TBaseResult>> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            Entries = entries.ToArray();

            if (Entries.Count == 0)
            {
                throw new ArgumentException("There are no entries.", nameof(entries));
            }

            var duplicateSubtypes = Entries
                .GroupBy(x => x.InputType)
                .Where(g => g.Count() > 1)
                .Select(x => x.Key)
                .ToArray();

            if (duplicateSubtypes.Length > 0)
            {
                var duplicateTypeList = string.Join<Type>(",", duplicateSubtypes);
                throw new ArgumentException(
                    $"Duplicate input types have appeared in polymorphic mapping entries: {duplicateTypeList}",
                    nameof(entries));
            }

            Entries = Entries.OrderBy(x => x.InputType, new DerivedFirstComparer()).ToArray();
        }

        /// <summary>
        /// The mapping entries, with the deepest subtypes first.
        /// </summary>
        public IReadOnlyList<PolymorphicMapEntry<TBaseInput, TBaseResult>> Entries { get; }

        /// <summary>
        /// Compares two types in order to position the deepest subtypes first.
        /// </summary>
        /// <example>
        /// Using the following types:
        /// <code>
        /// class Base {}
        /// class Derived : Base { }
        /// class Derived2 : Base { }
        /// class EvenMoreDerived : Derived { }
        /// </code>
        /// The types will be sorted as: EvenMoreDerived, Derived2, Derived, Base.
        /// </example>
        private class DerivedFirstComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                if (x == y)
                {
                    return 0; // The types are the same (this shouldn't happen?), ignore them. 
                }

                if (x.IsAssignableFrom(y))
                {
                    // x is assignable from y, which means that it must be the last in the list
                    // because it is less deep in the inheritance tree than y.

                    // For instance, let's assume we have two classes: Base and Derived.
                    // Base IsAssignableFrom Derived; so it must be last to avoid stealing
                    // Derived's specialized mapping.

                    return 1;
                }

                if (y.IsAssignableFrom(x))
                {
                    return -1; // The opposite happens here.
                }
                
                return 0; // The two types have no inheritance relationship; leave it as it is.
            }
        }
    }
}