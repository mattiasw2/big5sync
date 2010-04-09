using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    /// <summary>
    /// The exception is thrown when there is an error creating a folder.
    /// </summary>
    public class CreateFolderException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>CreateFolderException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>CreateFolderException</c> to be thrown.</param>
        public CreateFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_CREATE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
