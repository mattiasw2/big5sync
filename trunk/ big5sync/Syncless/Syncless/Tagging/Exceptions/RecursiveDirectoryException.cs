using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging.Exceptions
{
    public class RecursiveDirectoryException : Exception
    {
        private string _path;

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _tagname;

        public string Tagname
        {
            get { return _tagname; }
            set { _tagname = value; }
        }

        public RecursiveDirectoryException()
            : base()
        {
        }

        public RecursiveDirectoryException(string path, string tagname)
            : base()
        {
            this._path = path;
            this._tagname = tagname;
        }

        public RecursiveDirectoryException(string message, string path, string tagname)
            : base(message)
        {
            this._path = path;
            this._tagname = tagname;
        }
    }
}
