using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

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
            : base(ErrorMessage.TAG_ALREADY_EXISTS_EXCEPTION)
        {
            this._tagname = tagname;
        }
    }
}
