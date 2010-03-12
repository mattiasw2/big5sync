using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class MoveFolderException : ApplicationException
    {
        public MoveFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_MOVE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
