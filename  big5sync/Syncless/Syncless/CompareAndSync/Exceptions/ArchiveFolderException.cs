using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class ArchiveFolderException : ApplicationException
    {
        public ArchiveFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_ARCHIVE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
