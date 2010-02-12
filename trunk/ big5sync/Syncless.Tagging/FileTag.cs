using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class FileTag:Tag
    {
        public FileTag(string tagname)
            : base(tagname)
        {

        }
        
        public List<TaggedPath> FilePaths
        {
            get { return _pathList; }
        }

        public override bool AddPath(string path)
        {
            return AddPath(path, "");
        }
        
        public override bool AddPath(string path, string date)
        {
            if (!Contain(path))
            {
                TaggedPath taggedPath = new TaggedPath();
                taggedPath.Path = path;
                taggedPath.LogicalDriveId = GetLogicalID(path);
                taggedPath.Date = date;
                _pathList.Add(taggedPath);
                return true;
            }
            return false;
        }

        /*Private Implementation*/
        public override bool RemovePath(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    _pathList.Remove(p);
                    return true;
                }
            }
            return false;
        }

        protected override TaggedPath RetrieveTaggedPath(string path)
        {
            foreach (TaggedPath p in _pathList)
            {
                if (p.Path.Equals(path))
                {
                    return p;
                }
            }
            return null;
        }
    }
}
