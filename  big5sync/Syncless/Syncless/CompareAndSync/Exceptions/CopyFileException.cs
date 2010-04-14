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
    /// The exception is thrown when there is an error copying a file.
    /// </summary>
    public class CopyFileException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>CopyFileException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>CopyFileException</c> to be thrown.</param>
        public CopyFileException(Exception innerException) :
            base(ErrorMessage.CAS_UNABLE_TO_COPY_FILE_EXCEPTION, innerException)
        {
        }
    }
}
