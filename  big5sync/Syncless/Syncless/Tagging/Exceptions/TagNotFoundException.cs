using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging.Exceptions
{
    public class TagNotFoundException : Exception
    {
        private string _tagname;

        public string Tagname
        {
            get { return _tagname; }
        }
        
        public TagNotFoundException()
            : base()
        {
        }

        

        public TagNotFoundException(string tagname)
            : base(ErrorMessage.TAG_NOT_FOUND_EXCEPTION)
        {
            this._tagname = tagname;
        }
            
    }
}
