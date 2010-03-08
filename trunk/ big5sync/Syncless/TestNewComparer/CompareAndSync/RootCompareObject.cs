using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestNewComparer.CompareAndSync
{
    public class RootCompareObject : FolderCompareObject
    {
        private string[] _paths;

        public string[] Paths
        {
            get { return _paths; }
            set { _paths = value; }
        }
        public RootCompareObject(string[] paths):base("-root",paths.Length)
        {
            _paths = paths;
        }

    }
}
