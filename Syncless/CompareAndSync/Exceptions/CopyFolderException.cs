using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    /// <summary>
    /// The exception is thrown when there is an error copying a folder.
    /// </summary>
    public class CopyFolderException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>CopyFolderException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>CopyFolderException</c> to be thrown.</param>
        public CopyFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_COPY_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
