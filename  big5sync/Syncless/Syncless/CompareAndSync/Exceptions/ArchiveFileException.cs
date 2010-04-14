/*
 * 
 * Author: Soh Yuan Chin
 * 
 */

using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    /// <summary>
    /// The exception is thrown when there is an error with file archiving.
    /// </summary>
    public class ArchiveFileException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>ArchiveFileException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>ArchiveFileException</c> to be thrown.</param>
        public ArchiveFileException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_ARCHIVE_FILE_EXCEPTION, innerException)
        {
        }
    }
}
