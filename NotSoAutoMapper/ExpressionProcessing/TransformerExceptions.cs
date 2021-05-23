using System;

namespace NotSoAutoMapper.ExpressionProcessing
{
    internal static class TransformerExceptions
    {
        public static InvalidOperationException NullMapperException
            => new("The mapper is null.");
    }
}