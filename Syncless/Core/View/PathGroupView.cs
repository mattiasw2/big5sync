using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core.View
{
    public class PathGroupView
    {
        private List<string> _pathList;
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public List<string> PathList
        {
            get { return _pathList; }
            set { _pathList = value; }
        }

        public PathGroupView(string name)
        {
            _name = name;
            _pathList = new List<string>();
        }

        public void AddPath(string path)
        {
            _pathList.Add(path);
        }
    }
}
