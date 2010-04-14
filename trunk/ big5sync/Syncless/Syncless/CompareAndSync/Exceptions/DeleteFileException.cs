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
    /// The exception is thrown when there is an error deleting a file.
    /// </summary>
    public class DeleteFileException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>DeleteFileException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>DeleteFileException</c> to be thrown.</param>
        public DeleteFileException(Exception innerException) :
            base(ErrorMessage.CAS_UNABLE_TO_DELETE_FILE_EXCEPTION, innerException)
        {
        }
    }
}
