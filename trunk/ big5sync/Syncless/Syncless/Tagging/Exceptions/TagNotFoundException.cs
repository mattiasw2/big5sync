using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            : base()
        {
            this._tagname = tagname;
        }

        public TagNotFoundException(string message, string tagname)
            : base(message)
        {
            this._tagname = tagname;
        }
            
    }
}
