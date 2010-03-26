using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class DeleteFolderException : ApplicationException
    {
        public DeleteFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_DELETE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
