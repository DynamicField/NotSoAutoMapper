using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal class ReplacerVisitor : ExpressionVisitor
    {
        private readonly List<(Expression old, Expression @new)> _replacements =
            new();

        public ReplacerVisitor(Expression old, Expression @new)
        {
            CheckExpressions(old, @new);

            _replacements.Add((old, @new));
        }


        public ReplacerVisitor(IEnumerable<(Expression old, Expression @new)> replacements)
        {
            _replacements.AddRange(replacements);

            foreach (var (old, @new) in _replacements)
            {
                CheckExpressions(old, @new);
            }
        }

        public Expression Replace(Expression expr) => Visit(expr);

        public override Expression Visit(Expression node)
        {
            foreach (var (old, @new) in _replacements)
            {
                if (node == old)
                {
                    return base.Visit(@new)!;
                }
            }

            return base.Visit(node)!;
        }

        private static void CheckExpressions(Expression old, Expression @new)
        {
            if (old == @new)
            {
                throw new ArgumentException($"{nameof(old)} is the same as {nameof(@new)}.");
            }
        }
    }
}