using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging.Exceptions
{
    public class PathNotFoundException : Exception
    {
        private string _pathname;

        public string Pathname
        {
            get { return _pathname; }
        }

        public PathNotFoundException()
            : base()
        {
        }

        public PathNotFoundException(string pathname)
            : base()
        {
            this._pathname = pathname;
        }

        public PathNotFoundException(string message, string pathname)
            : base(message)
        {
            this._pathname = pathname;
        }
    }
}
