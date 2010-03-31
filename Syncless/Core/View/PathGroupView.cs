using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncless.Core.View
{
    public class PathGroupView
    {
        private List<PathView> _pathList;
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public List<PathView> PathList
        {
            get { return _pathList; }
            set { _pathList = value; }
        }
        public List<string> PathListString
        {
            get
            {
                List<string> pathListString = new List<string>();
                foreach (PathView p in _pathList)
                {
                    pathListString.Add((p.Path));
                }
                return pathListString;
            }
        }

            public PathGroupView(string name)
        {
            _name = name;
            _pathList = new List<PathView>();
        }

        public void AddPath(string path)
        {
            _pathList.Add(new PathView(path));
        }
        public void AddPath(PathView path)
        {
            _pathList.Add(path);
        }
    }
}
