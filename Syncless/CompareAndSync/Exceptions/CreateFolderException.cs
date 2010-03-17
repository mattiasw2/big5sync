using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class CreateFolderException : ApplicationException
    {
        public CreateFolderException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_CREATE_FOLDER_EXCEPTION, innerException)
        {
        }
    }
}
