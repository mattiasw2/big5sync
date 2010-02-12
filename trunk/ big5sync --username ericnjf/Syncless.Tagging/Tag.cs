using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public abstract class Tag
    {
        private string _tagName;
        public string TagName
        {
            get { return _tagName; }
            set { this._tagName = value; }
        }

        protected List<TaggedPath> _pathList;

        public Tag(string tagname)
        {
            TagName = tagname;
            this._pathList = new List<TaggedPath>();
        }

        public abstract Boolean AddPath(string path);
        public abstract Boolean AddPath(string path, string date);
        public bool Contain(string path)
        {
            return false;
        }
    }
}
