using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class CopyFileException : ApplicationException
    {
        public CopyFileException(Exception innerException) :
            base(ErrorMessage.CAS_UNABLE_TO_COPY_FILE_EXCEPTION, innerException)
        {
        }
    }
}
