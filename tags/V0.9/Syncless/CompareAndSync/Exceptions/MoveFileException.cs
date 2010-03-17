using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class MoveFileException : ApplicationException
    {
        public MoveFileException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_MOVE_FILE_EXCEPTION, innerException)
        {
        }
    }
}
