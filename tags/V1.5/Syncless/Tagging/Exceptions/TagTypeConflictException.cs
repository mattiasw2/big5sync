using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.Helper;

namespace Syncless.Tagging.Exceptions
{
    public class TagTypeConflictException : Exception
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

        private bool _isFolderTag;

        public bool IsFolderTag
        {
            get { return _isFolderTag; }
            set { _isFolderTag = value; }
        }

        public TagTypeConflictException()
            : base()
        {
        }

        

        public TagTypeConflictException(string path, string tagname, bool isFolderTag)
            : base(ErrorMessage.TAG_TYPE_CONFLICT_EXCEPTION)
        {
            this._path = path;
            this._tagname = tagname;
            this._isFolderTag = isFolderTag;
        }
    }
}
