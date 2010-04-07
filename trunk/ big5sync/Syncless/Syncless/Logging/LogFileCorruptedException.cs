using System;

namespace Syncless.Logging
{
    /// <summary>
    /// The exception that is thrown when the log file is corrupted.
    /// </summary>
    public class LogFileCorruptedException : ApplicationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.LogFileCorruptedException" /> class.
        /// </summary>
        public LogFileCorruptedException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Syncless.Logging.LogFileCorruptedException" /> class, given the message for this exception.
        /// </summary>
        /// <param name="message">A <see cref="string" /> specifying the message for this exception.</param>
        public LogFileCorruptedException(string message)
            : base(message)
        {
        }
    }
}