using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

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
            : base(ErrorMessage.PATH_NOT_FOUND_EXCEPTION)
        {
            this._pathname = pathname;
        }
    }
}
