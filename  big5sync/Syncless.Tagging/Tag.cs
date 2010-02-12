using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public abstract class Tag
    {
        protected string _tagName;

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

        public abstract bool AddPath(string path);
        
        public abstract bool AddPath(string path, string date);

        public bool Contain(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    return true;
                }
            }
            return false;
        }

        /*Private Implementation*/
        public abstract bool RemovePath(string path);

        protected string GetLogicalID(string path)
        {
            string[] tokens = path.Split(':');
            return tokens[0];
        }

        protected abstract TaggedPath RetrieveTaggedPath(string path);
    }
}
