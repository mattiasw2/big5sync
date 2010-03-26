using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class HashFileException : ApplicationException
    {
        public HashFileException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_HASH_EXCEPTION, innerException)
        {
        }
    }
}
