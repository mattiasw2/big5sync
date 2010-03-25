using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class DeleteFileException : ApplicationException
    {
        public DeleteFileException(Exception innerException) :
            base(ErrorMessage.CAS_UNABLE_TO_DELETE_FILE_EXCEPTION, innerException)
        {
        }
    }
}
