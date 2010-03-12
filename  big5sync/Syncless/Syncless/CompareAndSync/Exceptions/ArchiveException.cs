using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class ArchiveException : ApplicationException
    {
        public ArchiveException(Exception innerException)
            : base(ErrorMessage.CAS_UNABLE_TO_ARCHIVE_FILE_EXCEPTION, innerException)
        {
        }
    }
}
