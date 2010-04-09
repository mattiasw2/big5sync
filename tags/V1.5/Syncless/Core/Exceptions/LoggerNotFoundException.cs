using System;

namespace Syncless.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when the logger is not found
    /// </summary>
    class LoggerNotFoundException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the LoggerNotFoundException
        /// </summary>
        public LoggerNotFoundException()
        {
        }
        /// <summary>
        /// Initializes a new instance of the LoggerNotFoundException
        /// </summary>
        /// <param name="message">The message for the exception</param>
        public LoggerNotFoundException(string message)
            : base(message)
        {
        }
    }
}
