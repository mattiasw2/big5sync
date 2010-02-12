using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Tagging
{
    public class FolderTag:Tag
    {

        public FolderTag(string tagname):base(tagname){

        }
        public List<TaggedPath> FolderPaths
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
