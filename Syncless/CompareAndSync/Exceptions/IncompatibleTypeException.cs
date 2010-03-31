using System;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class IncompatibleTypeException : ApplicationException
    {
        public IncompatibleTypeException()
            : base(ErrorMessage.CAS_INCOMPATIBLE_TYPE_EXCEPTION)
        {
        }
    }
}
