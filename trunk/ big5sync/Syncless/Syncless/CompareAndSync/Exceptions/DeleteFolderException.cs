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
    /// The exception is thrown when there is an error deleting a folder.
    /// </summary>
    public class DeleteFolderException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>DeleteFolderException</c>, with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>DeleteFolderException</c> to be thrown.</param>
        public DeleteFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_DELETE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
