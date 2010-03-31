using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class CopyFolderException : ApplicationException
    {
        public CopyFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_COPY_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
