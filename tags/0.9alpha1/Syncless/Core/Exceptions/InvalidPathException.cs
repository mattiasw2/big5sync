using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;
namespace Syncless.Core.Exceptions
{
    public class InvalidPathException :Exception
    {
        private string _path;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public InvalidPathException(string path):base(ErrorMessage.INVALID_PATH)
        {
            this._path = path;
        }
    }
}
