using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.CompareAndSync
{
    public abstract class Request
    {
        protected List<string> _paths;
        protected bool _isFolder;

        public List<string> Paths
        {
            get { return _paths; }
        }

        public bool IsFolder
        {
            get { return _isFolder; }
            set { _isFolder = value; }
        }

    }
}
