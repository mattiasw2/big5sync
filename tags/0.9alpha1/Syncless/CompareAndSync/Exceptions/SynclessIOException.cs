using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.CompareAndSync.Exceptions
{
    public class SynclessIOException : ApplicationException
    {
        private string _from, _to;

        public SynclessIOException(string from, string to, string error, Exception innerException)
            : base(error, innerException)
        {
        }

        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        public string To
        {
            get { return _to; }
            set { _to = value; }
        }
    }
}
