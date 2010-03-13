using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
