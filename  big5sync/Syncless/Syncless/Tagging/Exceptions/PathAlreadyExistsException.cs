using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging.Exceptions
{
    public class PathAlreadyExistsException : Exception
    {
        private string _pathname;

        public string Pathname
        {
            get { return _pathname; }
        }

        public PathAlreadyExistsException()
            : base()
        {
        }

        public PathAlreadyExistsException(string pathname)
            : base()
        {
            this._pathname = pathname;
        }

        public PathAlreadyExistsException(string message, string pathname)
            : base(message)
        {
            this._pathname = pathname;
        }
    }
}
