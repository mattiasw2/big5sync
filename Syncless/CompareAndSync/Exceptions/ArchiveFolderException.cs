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
    /// The exception is thrown when there is an error archiving a folder.
    /// </summary>
    public class ArchiveFolderException : ApplicationException
    {
        /// <summary>
        /// Instantiates an instance of <c>ArchiveFolderException</c> with the actual exception passed in.
        /// </summary>
        /// <param name="innerException">The exception that caused <c>ArchiveFileException</c> to be thrown.</param>
        public ArchiveFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_ARCHIVE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
