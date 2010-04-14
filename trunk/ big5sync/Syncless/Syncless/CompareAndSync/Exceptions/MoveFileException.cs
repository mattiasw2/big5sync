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
    /// The exception is thrown when there is an error moving or renaming a file.
    /// </summary>
    public class MoveFileException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>MoveFileException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>MoveFileException</c> to be thrown.</param>
        public MoveFileException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_MOVE_FILE_EXCEPTION, innerException)
        {
        }
    }
}
