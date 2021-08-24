using System;

namespace NotSoAutoMapper.ExpressionProcessing
{
    /// <summary>
    /// Represents an error during expression transformation.
    /// </summary>
    public class ExpressionTransformationException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="ExpressionTransformationException"/>.
        /// </summary>
        public ExpressionTransformationException()
        {
        }

        /// <summary>
        /// Creates a new <see cref="ExpressionTransformationException"/> with the given message.
        /// </summary>
        /// <param name="message">The message.</param>
        public ExpressionTransformationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ExpressionTransformationException"/> with the given message and inner exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public ExpressionTransformationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}