using System;

namespace HandmadeMapper.ExpressionProcessing
{
    internal static class TransformerExceptions
    {
        public static InvalidOperationException RecursiveMapperException
            => new InvalidOperationException("Cannot recursively include the same mapper.");

        public static InvalidOperationException InvalidMapperException
            => new InvalidOperationException("The mapper is invalid.");
    }
}