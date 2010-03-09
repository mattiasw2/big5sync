using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAndSync.CompareObject
{
    public class FolderCompareObject : BaseCompareObject
    {
        private Dictionary<string, BaseCompareObject> _contents;

        public FolderCompareObject(string name, int numOfPaths)
            : base(name, numOfPaths)
        {
            _contents = new Dictionary<string, BaseCompareObject>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddChild(BaseCompareObject child)
        {
            _contents.Add(child.Name, child);
        }

        public BaseCompareObject GetChild(string name)
        {
            BaseCompareObject child = null;

            if (_contents.TryGetValue(name, out child))
                return child;
            else
                return child;
        }

        public bool ContainsChild(string name)
        {
            return _contents.ContainsKey(name);
        }

        public Dictionary<string, BaseCompareObject> Contents
        {
            get { return _contents; }
            set { _contents = value; }
        }
    }
}
