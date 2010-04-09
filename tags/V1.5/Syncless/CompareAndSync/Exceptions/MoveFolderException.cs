using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    /// <summary>
    /// The exception is thrown when there is an error moving or renaming a folder.
    /// </summary>
    public class MoveFolderException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>MoveFolderException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>MoveFolderException</c> to be thrown.</param>
        public MoveFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_MOVE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
