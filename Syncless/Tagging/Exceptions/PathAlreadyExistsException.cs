using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

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
            : base(ErrorMessage.PATH_ALREADY_EXISTS_EXCEPTION)
        {
            this._pathname = pathname;
        }
    }
}
