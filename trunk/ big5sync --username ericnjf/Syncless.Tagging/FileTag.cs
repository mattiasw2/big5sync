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

        public override Boolean AddPath(string path)
        {
            return false;
        }
        public override bool AddPath(string path, string date)
        {
            return false;
        }
    }
}
