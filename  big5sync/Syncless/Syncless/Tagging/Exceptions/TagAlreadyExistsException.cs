using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging.Exceptions
{
    public class TagAlreadyExistsException : Exception
    {
        private string _tagname;

        public string Tagname
        {
            get { return _tagname; }
        }

        public TagAlreadyExistsException()
            : base()
        {
        }

        public TagAlreadyExistsException(string tagname)
            : base()
        {
            this._tagname = tagname;
        }

        public TagAlreadyExistsException(string message, string tagname)
            : base(message)
        {
            this._tagname = tagname;
        }
    }
}
