using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public class RootCompareObject : FolderCompareObject
    {
        private string[] _paths;

        public RootCompareObject(string[] paths)
            : base(null, paths.Length)
        {
            _paths = paths;
        }

        public string[] Paths
        {
            get { return _paths; }
        }
    }
}
